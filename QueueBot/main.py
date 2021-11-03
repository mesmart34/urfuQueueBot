import telebot
from telebot import types

# def welcome_message(message):
#         print(message.text)
#         btns = bot.get_buttons([('b1', fun1), ('b2', fun2)])
#         markp = bot.add_keyboard(btns)
#         # markup = types.ReplyKeyboardMarkup(row_width=2)
#         # btn1 = types.KeyboardButton('b1')
#         # btn2 = types.KeyboardButton('b2')
#         # markup.add(btn1, btn2)
#         bot.send_message(message.chat.id, 'hi', reply_markup=markp)

def welcome(message):
    bot.send_message(message.chat.id, SRCGetter.get('src/welcome.txt'))

if __name__ == '__main__':
    commands = {
        'b1': lambda x: 'b1 pressed',
        'b2': lambda x: 'b2 pressed'
    }

    token = "2048641333:AAHhIyZxlWHN3By87p4KEo9-90bw4FBiy6s"
    bot = telebot.TeleBot(token)

    # @bot.message_handler(commands=['start'])
    # def welcome_message(message):
    #     print(message.text)
    #     markup = types.ReplyKeyboardMarkup(row_width=2)
    #     btn1 = types.KeyboardButton('b1')
    #     btn2 = types.KeyboardButton('b2')
    #     markup.add(btn1, btn2)
    #     bot.send_message(message.chat.id, 'hi', reply_markup=markup)
    #
    # @bot.message_handler()
    # def response(message):
    #     bot.send_message(message.chat.id, commands[message.text](0))

    bot.add_command('start', welcome_message)


    bot.infinity_polling(interval=0, timeout=20)

