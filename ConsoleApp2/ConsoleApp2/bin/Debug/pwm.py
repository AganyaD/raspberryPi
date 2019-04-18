import RPi.GPIO as GPIO
import time
#time.sleep(5)

print str("program start !!!.....")
GPIO.setmode(GPIO.BCM)
print str("init gpio as OUT")
GPIO.setup(5,GPIO.OUT)
GPIO.setup(6,GPIO.OUT)
GPIO.setup(13,GPIO.OUT)
GPIO.setup(26,GPIO.OUT)
GPIO.setup(19,GPIO.OUT)
GPIO.setup(21,GPIO.OUT)
GPIO.setup(23,GPIO.OUT)
GPIO.setup(24,GPIO.OUT)
print str("init gpio as pwm")
my_pwm_1 = GPIO.PWM(5,100)
my_pwm_2 = GPIO.PWM(6,100)
my_pwm_3 = GPIO.PWM(13,100)
my_pwm_4 = GPIO.PWM(26,100)
my_pwm_5 = GPIO.PWM(19,100)
my_pwm_6 = GPIO.PWM(21,100)
print str("start PWM")
my_pwm_1.start(0)
my_pwm_2.start(0)
my_pwm_3.start(0)
my_pwm_4.start(0)
my_pwm_5.start(0)
my_pwm_6.start(0)
print str("srart recive from file")
value1 = 0
value2 = 0
value3 = 0
value4 = 0
value5 = 0
value6 = 0
while(1):
        file = open("5", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        if value != value1:
           value1 = value
           my_pwm_1.ChangeDutyCycle(value1)
        file = open("6", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        if value != value2:
           value2 = value
           my_pwm_2.ChangeDutyCycle(value2)
        file = open("13", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        if value != value3:
           value3 = value
           my_pwm_3.ChangeDutyCycle(value3)
        file = open("26", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        if value != value4:
           value4 = value
           my_pwm_4.ChangeDutyCycle(value4)
        file = open("19", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        if value != value5:
           value5 = value
           my_pwm_5.ChangeDutyCycle(value5)
        file = open("21", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        if value != value6:
           value6 = value
           my_pwm_6.ChangeDutyCycle(value6) 

        file = open("exit", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        if value != "0":
           print("Exsit program")
           break

GPIO.cleanup()
