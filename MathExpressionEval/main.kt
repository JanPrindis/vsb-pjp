import java.lang.Exception

// Nodes
sealed class ExprNode {
    data class NumberNode(val value: Int) : ExprNode()
    data class AdditionNode(val left: ExprNode, val right: ExprNode) : ExprNode()
    data class SubtractionNode(val left: ExprNode, val right: ExprNode) : ExprNode()
    data class MultiplicationNode(val left: ExprNode, val right: ExprNode) : ExprNode()
    data class DivisionNode(val left: ExprNode, val right: ExprNode) : ExprNode()
}

class MathEvaluator {

    private lateinit var tokens: List<String>
    private var currentTokenIndex = 0

    /**
     * Splits string into tokens
     */
    private fun tokenize(expression: String): List<String> {

        val tokens = mutableListOf<String>()
        var currentToken = ""
        for(c in expression) {
            if(c == ' ') continue

            if(c.isDigit()) {
                currentToken += c
            }
            else {
                if(currentToken.isNotEmpty()) {
                    tokens.add(currentToken)
                    currentToken = ""
                }
                tokens.add(c.toString())
            }
        }
        if (currentToken.isNotEmpty()) {
            tokens.add(currentToken)
        }
        return tokens
    }

    /**
     * Attempts to evaluate expression, if not possible, prints 'ERROR'
     */
    fun evaluate(expression: String) {
        tokens = tokenize(expression)
        try {
            // Creates abstract syntax tree from tokens
            val ast = parseExpression()
            println("$expression = ${evaluateAst(ast)}")
        }
        catch(e: IllegalArgumentException) {
            // Missing bracket
            println("ERROR: ${e.message}")
        }
        catch(e: Exception) {
            // Wrong format
            println("ERROR")
        }
    }

    /**
     * Recursively evaluate AST
     */
    private fun evaluateAst(node: ExprNode): Int {
        return when(node) {
            is ExprNode.NumberNode -> node.value
            is ExprNode.AdditionNode -> evaluateAst(node.left) + evaluateAst(node.right)
            is ExprNode.DivisionNode -> evaluateAst(node.left) / evaluateAst(node.right)
            is ExprNode.MultiplicationNode -> evaluateAst(node.left) * evaluateAst(node.right)
            is ExprNode.SubtractionNode -> evaluateAst(node.left) - evaluateAst(node.right)
        }
    }

    /**
     * Handles + and -
     */
    private fun parseExpression(): ExprNode {
        // Check if term present
        var term = parseTerm()

        while(currentTokenIndex < tokens.size) {
            val operator = tokens[currentTokenIndex]
            if(operator == "+" || operator == "-") {
                currentTokenIndex++
                val nextTerm = parseTerm()

                term = if(operator == "+") {
                    ExprNode.AdditionNode(term, nextTerm)
                } else {
                    ExprNode.SubtractionNode(term, nextTerm)
                }
            }
            // If current index is not expression, ignore
            else break
        }

        return term
    }

    /**
     * Handles * and /
     */
    private fun parseTerm(): ExprNode {
        // Check if factor present
        var factor = parseFactor()

        while(currentTokenIndex < tokens.size) {
            val operator = tokens[currentTokenIndex]
            if(operator == "*" || operator == "/") {
                currentTokenIndex++
                val nextFactor = parseFactor()
                factor = if(operator == "*") {
                    ExprNode.MultiplicationNode(factor, nextFactor)
                } else {
                    ExprNode.DivisionNode(factor, nextFactor)
                }
            }
            // If current index is not term, ignore
            else break
        }

        return factor
    }

    /**
     * Handles brackets
     */
    private fun parseFactor(): ExprNode {
        val currentToken = tokens[currentTokenIndex]
        return if(currentToken == "(") {
            currentTokenIndex++

            val exprInside = parseExpression()

            // Consume closing bracket
            if(currentTokenIndex < tokens.size && tokens[currentTokenIndex] == ")") {
                currentTokenIndex ++
                exprInside
            }
            // If on end of expression and closing bracket missing, throw error
            else throw IllegalArgumentException("Mismatched parentheses")
        }
        else {
            currentTokenIndex++ // Move index to next token

            // If current index is not bracket, it must be a number
            // If not, it will trigger catch() inside evaluate() which will result in 'ERROR' being outputted
            ExprNode.NumberNode(currentToken.toInt())
        }
    }
}

fun main() {
    val evaluator = MathEvaluator()

    // Num of lines
    val numOfExp = readln()
    for(i in 0..<numOfExp.toInt()) {
        // Get expression string from input and evaluate
        val input = readln()
        evaluator.evaluate(input)
    }
}