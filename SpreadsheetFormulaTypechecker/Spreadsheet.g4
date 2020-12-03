grammar Spreadsheet;

/*
 * Parser Rules
 */

spreadSheet
	:	statements+=stm+
	;

stm
	: ';'															#emptyStm
	| 'C[' left=exp ',' right=exp ']' 'is' tp=type					#cellTypeStm
	| 'C[' left=exp ',' right=exp ']' '=' content=exp				#cellValueStm
	| 'C[' left=exp ',' right=exp ']' '=' '{' content=exp '}'		#cellFormulaStm
	| tp=type IDENT '=' val=exp										#assignStm
	| 'eval'														#evalStm
	| 'if' check=exp 'then' trueStm=stm 'else' falseStm=stm			#ifStm
	| 'while' check=exp 'do' loopStm=stm							#whileStm
	;


exp
	: fun=fexp														#functionExp
	| val=value														#valueExp
	| IDENT															#varExp
	| 'C[' left=aexp ',' right=aexp ']'								#cellExp
	| '(' expr=exp ')'												#bracketExp
	| left=exp '*' right=exp										#multExp
	| left=exp '/' right=exp										#divExp
	| left=exp '%' right=exp										#modExp
	| left=exp '+' right=exp										#addExp
	| left=exp '-' right=exp										#subExp				
	| left=exp '&' right=exp										#concatExp
	| left=exp '<' right=exp										#smallerExp
	| left=exp '>' right=exp										#greaterExp
	| left=exp '<=' right=exp										#smallerEqExp
	| left=exp '>=' right=exp										#greaterEqExp
	| left=exp '==' right=exp										#equalExp
	| left=exp '!=' right=exp										#unequalExp
	| left=exp '&&' right=exp										#andExp
	| left=exp '||' right=exp										#orExp
	| '!' param=exp													#notExp
	| '+' param=exp													#posExp
	| '-' param=exp													#negExp
	;

fexp
	: 'IF' threeArg													#ifFunc
	| 'ISBLANK' oneArg												#isblankFunc
	| 'ISNA' oneArg													#isnaFunc
	| 'SUM' anyArg													#sumFunc
	| 'PROD' anyArg													#prodFunc
	| 'AND' anyArg													#andFunc
	| 'OR' anyArg													#orFunc
	| 'AVERAGE' anyArg												#averageFunc
	| 'MAX' anyArg													#maxFunc
	| 'MIN' anyArg													#minFunc
	| 'ROUNDUP' twoArg												#roundupFunc
	| 'ROUND' twoArg												#roundFunc
	| 'N' oneArg													#nFunc
	;

oneArg
	: '(' exp ')'
	;

twoArg
	: '(' first=exp ',' second=exp ')'
	;

threeArg
	: '(' first=exp ',' second=exp ',' third=exp ')'
	;

anyArg
	: '(' expr+=exp (',' expr+=exp)* ')'
	;

aexp
	: '->' param=exp												#posAExp
	| '<-' param=exp												#negAExp
	| param=exp														#baseAExp
	;

type
	: 'bool'														#boolTp
	| 'int'															#intTp
	| 'decimal'														#decimalTp
	| 'string'														#stringTp
	| 'currency'													#currencyTp
	| 'date'														#dateTp
	;

value
	: 'true'														#trueVal
	| 'false'														#falseVal
	| INT															#intVal
	| DECIMAL														#decimalVal
	| EMPTY															#emptyVal
	| STRING														#stringVal
	| DOLLARS														#dollarsVal
	| EUROS															#eurosVal
	| DATE															#dateVal
	;



/*
 * Lexer Rules
 */

WS
	:	' ' -> skip
	;

END
	:	EOF -> skip
	;

NL
	:	[\n\r] -> skip
	;

COMMENT
    :   ( '//' ~[\r\n]* '\r'? '\n'
        | '/*' .*? '*/'
        ) -> skip
    ;

EMPTY
	: '\\'
	;

IDENT
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;

INT     
	: [-+]?[0-9]+
	;

DECIMAL     
	: [-+]?[0-9]+ [.] [0-9]+
	;

EUROS
	: NUMBER '€'
	;

DOLLARS
	: '$' NUMBER
	;

STRING
    : '"' ( EscapeSequence | ~('\\'|'"') )* '"'
    ;

DATE
	: CALENDARDATE 'T' DATEDAYTIME
	;

CALENDARDATE
	: NONFRACTPOSNUMBER '-' MONTHNUMBER '-' DAYNUMBER
	;

DATEDAYTIME
	: TIMENUMBER ':' TIMENUMBER ':' TIMENUMBER
	;

fragment EscapeSequence
    : '\\' [abfnrtvz"'\\]
    | '\\' '\r'? '\n'
	;

fragment NUMBER
	: [-+]? POSNUMBER
	;

fragment POSNUMBER
	: NONFRACTPOSNUMBER ([.,] [0-9]+)?
	;

fragment NONFRACTPOSNUMBER
	: [1-9][0-9]*
	;

fragment MONTHNUMBER
	: [01]?[0-9]
	;

fragment DAYNUMBER
	: [0123]?[0-9]
	;

fragment TIMENUMBER
	: [0-6]?[0-9]
	;