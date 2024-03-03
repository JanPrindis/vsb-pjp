import java.io.File
import java.io.InputStream

class Scanner(filepath: String) {
    private val inputStream: InputStream = File(filepath).inputStream()
    private val fileString = inputStream.bufferedReader().use { it.readText() }
    private var position = 0

    fun nextToken(): Token {
        if(position >= fileString.length) {
            return Token(TokenType.EOF, null)
        }

        val current = fileString[position]
        position++

        if(current == ' ') return nextToken()

        when(current) {
            '+' -> return Token(TokenType.OP, "+")
            '-' -> return Token(TokenType.OP, "-")
            '*' -> return Token(TokenType.OP, "*")
            '/' -> {
                return if(position < fileString.length && fileString[position] == '/') {
                    // Is comment
                    while(position < fileString.length) {
                        if(fileString[position] != '\n')
                            position++
                        else {
                            position++
                            break
                        }
                    }
                    nextToken()
                } else {
                    Token(TokenType.OP, "/")
                }
            }
            '(' -> return Token(TokenType.LPAR, null)
            ')' -> return Token(TokenType.RPAR, null)
            ';' -> return Token(TokenType.SEMICOLON, null)
            else -> {}
        }
        if(current.isDigit()) {
            // Is NUM
            var value = current.toString()

            while (position < fileString.length && fileString[position].isDigit()) {
                value += fileString[position].toString()
                position++
            }

            return Token(TokenType.NUM, value)
        }
        else {
            // Is ID
            var value = current.toString()

            while (position < fileString.length && fileString[position].isLetter()) {
                value += fileString[position].toString()
                position++

                if (value.length == 3) {
                    if (value.lowercase() == "mod") {
                        return Token(TokenType.MOD, null)
                    }
                    if (value.lowercase() == "div") {
                        return Token(TokenType.DIV, null)
                    }
                }
            }
            return Token(TokenType.ID, value)
        }
    }
}