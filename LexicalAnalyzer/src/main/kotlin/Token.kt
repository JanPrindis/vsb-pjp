enum class TokenType {
    EOF,
    NUM,
    OP,
    LPAR,
    RPAR,
    DIV,
    MOD,
    SEMICOLON,
    ID
}

data class Token(val type: TokenType, val value: String?) {
    override fun toString(): String {
        return when (this.type) {
            TokenType.NUM -> "NUM:${this.value}"
            TokenType.OP -> "OP:${this.value}"
            TokenType.LPAR -> "LPAR"
            TokenType.RPAR -> "RPAR"
            TokenType.DIV -> "DIV"
            TokenType.MOD -> "MOD"
            TokenType.SEMICOLON -> "SEMICOLON"
            TokenType.ID -> "ID:${this.value}"
            else -> "EOF"
        }
    }
}