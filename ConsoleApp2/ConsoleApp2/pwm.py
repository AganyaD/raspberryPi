import RPi.GPIO as GPIO

str("program start !!!.....")
GPIO.setmode(GPIO.BCM)
str("init gpio as OUT")
GPIO.setup(5,GPIO.OUT)
GPIO.setup(6,GPIO.OUT)
GPIO.setup(13,GPIO.OUT)
GPIO.setup(26,GPIO.OUT)
GPIO.setup(19,GPIO.OUT)
GPIO.setup(21,GPIO.OUT)
str("init gpio as pwm")
my_pwm_1 = GPIO.PWM(5,100)
my_pwm_2 = GPIO.PWM(6,100)
my_pwm_3 = GPIO.PWM(13,100)
my_pwm_4 = GPIO.PWM(26,100)
my_pwm_5 = GPIO.PWM(19,100)
my_pwm_6 = GPIO.PWM(21,100)
str("start PWM")
my_pwm_1.start(50)
my_pwm_2.start(50)
my_pwm_3.start(50)
my_pwm_4.start(50)
my_pwm_5.start(50)
my_pwm_6.start(50)
str("srart recive from user")
while(1):
        dutyCycle = input("Enter duty cycle ")
        my_pwm_1.ChangeDutyCycle(dutyCycle)
        my_pwm_2.ChangeDutyCycle(dutyCycle)
        my_pwm_3.ChangeDutyCycle(dutyCycle)
        my_pwm_4.ChangeDutyCycle(dutyCycle)
        my_pwm_5.ChangeDutyCycle(dutyCycle)
        my_pwm_6.ChangeDutyCycle(dutyCycle)

GPIO.cleanup()
