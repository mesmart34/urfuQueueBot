import telebot
from telebot import types

if __name__ == '__main__':
    commands = {
        'b1': lambda: 'b1 pressed',
        'b2': lambda: 'b2 pressed'
    }

    token = "2048641333:AAHhIyZxlWHN3By87p4KEo9-90bw4FBiy6s"
    bot = telebot.TeleBot(token)

    @bot.message_handler(commands=['start'])
    def welcome_message(message):
        print(message.text)
        markup = types.ReplyKeyboardMarkup(row_width=2)
        btn1 = types.KeyboardButton('b1')
        btn2 = types.KeyboardButton('b2')
        markup.add(btn1, btn2)
        bot.send_message(message.chat.id, 'hi', reply_markup=markup)

    @bot.message_handler()
    def response(message):
        bot.send_message(message.chat.id, commands[message.text](0))

    bot.infinity_polling(interval=0, timeout=20)

