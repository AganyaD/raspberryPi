import RPi.GPIO as GPIO
import time
time.sleep(5)

print str("program start !!!.....")
GPIO.setmode(GPIO.BCM)
print str("init gpio as OUT")
GPIO.setup(5,GPIO.OUT)
GPIO.setup(6,GPIO.OUT)
GPIO.setup(13,GPIO.OUT)
GPIO.setup(26,GPIO.OUT)
GPIO.setup(19,GPIO.OUT)
GPIO.setup(21,GPIO.OUT)
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
value = 0
while(1):
        file = open("5", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        my_pwm_1.ChangeDutyCycle(value)
        file = open("6", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        my_pwm_2.ChangeDutyCycle(value)
        file = open("13", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        my_pwm_3.ChangeDutyCycle(value)
        file = open("26", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        my_pwm_4.ChangeDutyCycle(value)
        file = open("19", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        my_pwm_5.ChangeDutyCycle(value)
        file = open("21", "r")
        try:
           value = int(file.read())
        except:
              print("Something went wrong")
        my_pwm_6.ChangeDutyCycle(value)

GPIO.cleanup()
