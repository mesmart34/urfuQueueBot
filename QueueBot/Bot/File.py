class File():
    def __init__(self, path: str, f_type: str, caption: str = ''):
        self.__path = path
        self.__type = f_type
        self.__caption = caption

    def get_path(self):
        return self.__path

    def get_type(self):
        return self.__type

    def get_caption(self):
        return self.__caption
