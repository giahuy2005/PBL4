import signal
import time
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
async def try_open_camera(url, timeout=3):
    def open_cam():
        cap = cv2.VideoCapture(url)
        ok = cap.isOpened()
        cap.release()
        return ok

    try:
        return await asyncio.wait_for(asyncio.to_thread(open_cam), timeout=timeout)
    except asyncio.TimeoutError:
        logging.error("Camera open timeout")
        return False
async def stream_camera(websocket):
    logging.info("Client connected")
    connections.add(websocket)
    cameras = {}
    current_checks = {}
    send_lock = asyncio.Lock()
    try:
        async for message in websocket:
            data = json.loads(message)
            cmd = data.get("cmd")
            cam_id = data.get("camera")
            url = data.get("url")
            if cmd == "add" and cam_id not in cameras:
                isopen=await  try_open_camera(url)
                if isopen==False:
                    logging.error(f"Không mở được camera {cam_id}")
                    await websocket.send(json.dumps({"cmd":"add_failed","camera":cam_id,"error":"Không mở được camera"}))
                    continue
                frame_queue = queue.Queue(maxsize=5)
                stop_flag = threading.Event()
                cam_thread = CameraThread(cam_id,url,frame_queue,stop_flag)
                cam_thread.start()
                task = asyncio.create_task(handle_send(websocket, frame_queue, send_lock))
                cameras[cam_id] = (cam_thread, stop_flag, frame_queue, task)
                logging.info(f"Added camera {cam_id}")
                await websocket.send(json.dumps({"cmd":"add_success","camera":cam_id}))
            elif cmd == "stop" and cam_id in cameras:
                cam_thread, stop_flag, _, task = cameras.pop(cam_id)
                stop_flag.set()
                task.cancel()
                logging.info(f"Stopped camera {cam_id}")
            elif cmd == "check": 
                if cam_id in current_checks:
                    current_checks[cam_id].cancel()
                async def do_check():
                    try:
                        is_open = await try_open_camera(url,timeout=10)
                        response = {
                            "cmd": "check_response",
                            "status": "ok" if is_open else "fail",
                            "camera": cam_id,
                            "url": url
                        }
                        if not is_open:
                            response["error"] = "Camera URL cannot be opened."
                        try: 
                            await websocket.send(json.dumps(response))
                        except websockets.ConnectionClosed:
                            pass
                        logging.info(f"Checked camera URL {url}: {'Success' if is_open else 'Failed'}")
                    except asyncio.CancelledError:
                        logging.info(f"Check camera {cam_id} bị hủy giữa chừng")
                    finally:
                        current_checks.pop(cam_id, None)
                task = asyncio.create_task(do_check())
                current_checks[cam_id] = task
            elif cmd == "cancel_check" and cam_id in current_checks:
                current_checks[cam_id].cancel()
                await websocket.send(json.dumps({
                    "cmd": "check_cancelled",
                    "camera": cam_id
                }))
                logging.info(f"User cancelled check for camera {cam_id}")
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
