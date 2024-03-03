import java.nio.file.Paths

fun main() {
    val path = Paths.get("").toAbsolutePath().toString() + "/test.txt"
    val scanner = Scanner(path)
    var token = scanner.nextToken()

    while (token.type != TokenType.EOF) {
        println(token.toString())
        token = scanner.nextToken()
    }
}