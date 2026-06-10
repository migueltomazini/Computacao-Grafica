from OpenGL.GL import *

class Shader:
    """
    Classe Wrapper para encapsular a burocracia de instanciamento, carregamento, 
    compilação e tratamento de erros do fluxo nativo C-like da API de Shaders do OpenGL.
    """
    def __init__(self, vertexPath: str, fragmentPath: str):
        try:
            # 1. Recuperação do Código Fonte em Arquivos Físicos
            vShaderFile = open(vertexPath)
            fShaderFile = open(fragmentPath)
            
            vertexCode = vShaderFile.read()
            fragmentCode = fShaderFile.read()
            
            vShaderFile.close()
            fShaderFile.close()

            # 2. Compilação do Processador Geométrico (Vertex)
            vertex = glCreateShader(GL_VERTEX_SHADER)
            glShaderSource(vertex, vertexCode)
            glCompileShader(vertex)
            self.checkCompileErrors(vertex, "VERTEX")
            
            # 3. Compilação do Processador Fotométrico (Fragment)
            fragment = glCreateShader(GL_FRAGMENT_SHADER)
            glShaderSource(fragment, fragmentCode)
            glCompileShader(fragment)
            self.checkCompileErrors(fragment, "FRAGMENT")
            
            # 4. Linkagem do Pipeline Final
            self.ID = glCreateProgram()
            glAttachShader(self.ID, vertex)
            glAttachShader(self.ID, fragment)
            glLinkProgram(self.ID)
            self.checkCompileErrors(self.ID, "PROGRAM")
            
            # Limpeza C-Like
            glDeleteShader(vertex)
            glDeleteShader(fragment)
        
        except IOError:
            print("ERROR::SHADER::FILE_NOT_SUCCESFULLY_READ")

    def getProgram(self):
        """Retorna o inteiro alocado para o uso do Programa do Pipeline."""
        return self.ID
        
    def use(self) -> None:
        """Ativa este Shader para a atual máquina de estado do OpenGL."""
        glUseProgram(self.ID)
        
    def setBool(self, name: str, value: bool) -> None:
        """Configurador rápido de variáveis espaciais Booleanas."""
        glUniform1i(glGetUniformLocation(self.ID, name), int(value))
        
    def setInt(self, name: str, value: int) -> None:
        """Configurador rápido de variáveis espaciais Numéricas Simples (Int)."""
        glUniform1i(glGetUniformLocation(self.ID, name), value)
        
    def setFloat(self, name: str, value: float) -> None:
        """Configurador rápido de variáveis espaciais Flutuantes."""
        glUniform1f(glGetUniformLocation(self.ID, name), value)

    def checkCompileErrors(self, shader: int, type: str) -> None:
        """
        Intermediador de Depuração (Catch). 
        Impede falhas silenciosas emitindo Tracebacks descritivos nativos das bibliotecas GLSL.
        """
        if (type != "PROGRAM"):
            success = glGetShaderiv(shader, GL_COMPILE_STATUS)
            if (not success):
                infoLog = glGetShaderInfoLog(shader)
                print("ERROR::SHADER_COMPILATION_ERROR of type: " + type + "\n" + infoLog.decode() + "\n -- --------------------------------------------------- -- ")
        else:
            success = glGetProgramiv(shader, GL_LINK_STATUS)
            if (not success):
                infoLog = glGetProgramInfoLog(shader)
                print("ERROR::PROGRAM_LINKING_ERROR of type: " + type + "\n" + infoLog.decode() + "\n -- --------------------------------------------------- -- ")