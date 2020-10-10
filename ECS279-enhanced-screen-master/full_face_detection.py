import cv2
import numpy as np
import dlib
import socket
import time
import math

from typing import List
from collections import deque

# from scipy.interpolate import interp1d

class SocketClient:
    """
    SocketClient to handle socket stuff
    """

    def __init__(self):
        self.UDP_IP = "127.0.0.1"
        self.UDP_PORT = 5065
        print("UDP target IP: {}".format(self.UDP_IP))
        print("UDP target port: {}".format(self.UDP_PORT))
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    
    def send_info(self, info: List[float]):
        x, y, area = info
        bin_data = bytes("{},{},{}".format(x, y, area), encoding='utf-8')
        print("Sending {}".format(info))
        self.sock.sendto(bin_data, (self.UDP_IP, self.UDP_PORT))

    def send_null(self):
        bin_data = bytes("{},{},{}".format('no', 'no', 'no'), encoding='utf-8')
        self.sock.sendto(bin_data, (self.UDP_IP, self.UDP_PORT))


def dist(point1, point2) -> float:
    return math.sqrt((point1.x - point2.x) ** 2 + (point1.y - point2.y) ** 2)


def area(
    left_point, 
    right_point,
    top_point,
    bottom_point
) -> float:
    return dist(left_point, right_point) * dist(top_point, bottom_point)


if __name__ == "__main__":
    cap = cv2.VideoCapture(0)

    detector = dlib.get_frontal_face_detector()
    predictor = dlib.shape_predictor("shape_predictor_68_face_landmarks.dat")

    s = SocketClient()
    num_frame = 0

    start = time.time()
    while True:
        num_frame += 1
        _, frame = cap.read()
        resized = cv2.resize(frame, (16 * 30, 9 * 30))
        gray = cv2.cvtColor(resized, cv2.COLOR_BGR2GRAY)

        faces = detector(gray)      
        if len(faces) == 0:
            s.send_null()
            print("no message sent.")
        end = time.time()
        for face in faces:
            landmarks = predictor(gray, face)
            face_area = area(
                landmarks.part(0),
                landmarks.part(16),
                landmarks.part(27),
                landmarks.part(8)
            )
            # draw point
            x = landmarks.part(33).x
            y = landmarks.part(33).y
            cv2.circle(resized, (x, y), 2, (0, 0, 255), -1)
            s.send_info([x, y, face_area])
        
        print("fps: {0: .2f}".format(num_frame / (end - start)))
        
        cv2.imshow("Frame", resized)

        key = cv2.waitKey(1)
        if key == 27:
            break