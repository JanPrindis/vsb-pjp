grammar ProjectGrammar;

prog: stat+ EOF ; // Start rule

block : '{' stat+ '}' ;

stat
    : (';')+                            #empty
    | variableType ID (',' ID)* (';')+  #declaration
    | expr (';')+                       #expression
    | READ_KW ID (',' ID)* (';')+       #read
    | WRITE_KW expr (',' expr)* (';')+  #write
    | COMMENT                           #comment
    | loop                              #loops
    | cond                              #condition
    | block                             #blockOfStatements
    ;
    
expr
    : INT                               #int
    | FLOAT                             #float
    | STRING                            #string
    | BOOL                              #bool
    | ID                                #id
    | SUB expr                          #unaryMinus
    | NOT expr                          #not
    | expr op=(MUL|DIV|MOD) expr        #mulDivMod
    | expr op=(ADD|SUB|CONCAT) expr     #addSubConcat
    | expr op=(EQ|NOTEQ) expr           #eqNeq
    | expr op=(LT|GT) expr              #ltGt
    | expr AND expr                     #and
    | expr OR expr                      #or
    | '(' expr ')'                      #parentheses
    | expr QM expr COLON expr           #ternaryCondCond
    | <assoc=right> ID '=' expr         #assignment
    ;
    
    
cond
    : IF_KW '(' expr ')' stat                   #ifStat
    | IF_KW '(' expr ')' stat ELSE_KW stat      #ifStatElseStat
    | IF_KW '(' expr ')' stat ELSE_KW block     #ifStatElseBlock
    | IF_KW '(' expr ')' block                  #ifBlock
    | IF_KW '(' expr ')' block ELSE_KW stat     #ifBlockElseStat
    | IF_KW '(' expr ')' block ELSE_KW block    #ifBlockElseBlock
    ;
    
loop
    : WHILE_KW '(' expr ')' stat                    #whileStat
    | WHILE_KW '(' expr ')' block                   #whileBlock
    | DO_KW stat WHILE_KW '(' expr ')' (';')+       #doWhileStat
    | DO_KW block WHILE_KW '(' expr ')' (';')+      #doWhileBlock    
    ;

variableType
    : type=INT_KW
    | type=FLOAT_KW
    | type=BOOL_KW
    | type=STRING_KW
    ;

// Values
FLOAT :     [0-9]+ '.' [0-9]+ ;
INT :       [0-9]+ ;                           
BOOL :      ('true'|'false') ;
STRING :    '"' ( '\\' . | ~["\\] )* '"';

// Type keywords
INT_KW :    'int' ;
FLOAT_KW :  'float' ;
BOOL_KW :   'bool' ;
STRING_KW : 'string' ;

// Other keywords
READ_KW :   'read' ; 
WRITE_KW :  'write' ;
IF_KW :     'if' ;
ELSE_KW :   'else' ;
WHILE_KW :  'while' ;
DO_KW :     'do' ;

// Operators
ADD :       '+' ;
SUB :       '-' ;
MUL :       '*' ;
DIV :       '/' ;
MOD :       '%' ;
CONCAT :    '.' ;
GT :        '>' ;
LT :        '<' ;
EQ :        '==' ;
NOTEQ :     '!=' ;
AND :       '&&' ;
OR :        '||' ;
NOT :       '!' ;
QM:         '?' ;
COLON:      ':' ;

// Variable IDs
ID : [a-zA-Z][a-zA-Z0-9]* ;

// Commments
COMMENT: '//' ~[\r\n]* -> skip ;

// Whitespace trim
WS : [ \t\r\n]+ -> skip ;
