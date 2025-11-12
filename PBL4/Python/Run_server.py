import signal
import cv2
import asyncio
import websockets
import base64
import json
import logging
import threading
import queue
import os

logging.basicConfig(
    filename="camera_server.log",
    level=logging.INFO,
    filemode="w",
    format="%(asctime)s [%(levelname)s] %(message)s"
)

connections = set()
stop_event = asyncio.Event()  
class CameraThread(threading.Thread):
    def __init__(self, cam_id, url, frame_queue, stop_flag):
        super().__init__(daemon=True)
        self.cam_id = cam_id
        self.url = url
        self.frame_queue = frame_queue
        self.stop_flag = stop_flag
    def run(self):
        logging.info(f"Thread start camera {self.cam_id}")
        cap = cv2.VideoCapture(self.url)
        if not cap.isOpened():
            logging.error(f"Không mở được camera {self.cam_id}")
            add_is = False
            return 
        while not self.stop_flag.is_set():
            ret, frame = cap.read()
            if not ret:
                logging.warning(f"Camera {self.cam_id} mất tín hiệu")
                break
            _, buffer = cv2.imencode(".jpg", frame)
            jpg_as_text = base64.b64encode(buffer).decode("utf-8")
            try:
                self.frame_queue.put_nowait({
                    "camera": self.cam_id,
                    "frame": jpg_as_text
                })
            except queue.Full:
                pass  
        cap.release()
        logging.info(f"Thread stop camera {self.cam_id}")
async def handle_send(websocket, frame_queue, send_lock):
    count = 0
    while not stop_event.is_set():
        try:
            frame_data = await asyncio.to_thread(frame_queue.get)
            async with send_lock:
                await websocket.send(json.dumps(frame_data))
                count += 1
                if count % 30 == 0:  
                    logging.info(f"Đã gửi {count} frame tới client")
        except Exception as e:
            logging.error(f"Gửi frame lỗi: {e}")
            break

async def stream_camera(websocket):
    logging.info("Client connected")
    connections.add(websocket)
    cameras = {}
    send_lock = asyncio.Lock()
    try:
        async for message in websocket:
            data = json.loads(message)
            cmd = data.get("cmd")
            cam_id = data.get("camera")
            url = data.get("url")
            if cmd == "add" and cam_id not in cameras:
                probe = cv2.VideoCapture(url)
                if not probe.isOpened():
                    logging.error(f"Không mở được camera {cam_id}")
                    await websocket.send(json.dumps({"camera": cam_id, "status": "error"}))
                    continue
                probe.release()
                frame_queue = queue.Queue(maxsize=5)
                stop_flag = threading.Event()
                cam_thread = CameraThread(cam_id,url,frame_queue,stop_flag)
                cam_thread.start()
                task = asyncio.create_task(handle_send(websocket, frame_queue, send_lock))
                cameras[cam_id] = (cam_thread, stop_flag, frame_queue, task)
                logging.info(f"Added camera {cam_id}")
            elif cmd == "stop" and cam_id in cameras:
                cam_thread, stop_flag, _, task = cameras.pop(cam_id)
                stop_flag.set()
                task.cancel()
                logging.info(f"Stopped camera {cam_id}")
            elif cmd == "shutdown":
                logging.info("Received shutdown command")
                stop_event.set()
                break
    except websockets.ConnectionClosed:
        logging.info("Client disconnected")
    finally:
        for cam_thread, stop_flag, _, task in cameras.values():
            stop_flag.set()
            task.cancel()
        connections.discard(websocket)
        logging.info("Cleaned up client tasks")
def shutdown_signal(*_):
    logging.info("Received system signal — shutting down...")
    stop_event.set()
signal.signal(signal.SIGTERM, shutdown_signal)
signal.signal(signal.SIGINT, shutdown_signal)
async def main():
    logging.info("Server starting ws://0.0.0.0:36000")
    server = await websockets.serve(stream_camera, "0.0.0.0", 36000)
    await stop_event.wait()
    logging.info("Shutting down server...")
    server.close()
    await server.wait_closed()
    for conn in list(connections):
        try:
            await conn.close()
        except:
            pass
    logging.info("Server stopped cleanly.")
    os._exit(0)
if __name__ == "__main__":
    asyncio.run(main())
