class File():
    def __init__(self, path: str, f_type: str):
        self.__path = path
        self.__type = f_type

    def get_path(self):
        return self.__path

    def get_type(self):
        return self.__type
