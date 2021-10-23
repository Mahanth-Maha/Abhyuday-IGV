import socket
import time
import random

host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))

startPos = [0, 0, 0] #Vector3   x = 0, y = 0, z = 0
timing = 0
LeftOrRight = 0.0
MoveOrNot = 0.5
while True:
    time.sleep(1/60) 
    if timing < 60*30:
        timing+=1
        LeftOrRight = 0.7
    else:
        LeftOrRight = 0.0
    #startPos[0] += 1 #increase x by one
    startPos[0] = LeftOrRight  
    startPos[1] = MoveOrNot
    posString = ','.join(map(str, startPos)) #Converting Vector3 to a string, example "0,0,0"
    print(posString)

    sock.sendall(posString.encode("UTF-8")) #Converting string to Byte, and sending it to C#
    receivedData = sock.recv(1024).decode("UTF-8") #receiveing data in Byte fron C#, and converting it to String
    print(receivedData)
    
    
sock.shutdown(socket.SHUT_RDWR)