grammar ExcelFormula;

/*
 * Parser Rules
 */

excelExpr
	: '=' expr=exp
	;

exp
	: fun=fexp														#functionExp
	| cell=celladress												#cellExp
	| val=value														#valueExp
	| left=exp '*' right=exp										#multExp
	| left=exp '/' right=exp										#divExp
	| left=exp '%' right=exp										#modExp
	| left=exp '+' right=exp										#addExp
	| left=exp '-' right=exp										#subExp				
	| left=exp '<' right=exp										#smallerExp
	| left=exp '>' right=exp										#greaterExp
	| left=exp '<=' right=exp										#smallerEqExp
	| left=exp '>=' right=exp										#greaterEqExp
	| left=exp '==' right=exp										#equalExp
	| left=exp '!=' right=exp										#unequalExp
	| left=exp '&&' right=exp										#andExp
	| left=exp '||' right=exp										#orExp
	| '!' param=exp													#notExp
	;

fexp
	: 'IF' threeArg													#ifFunc
	| 'ISBLANK' oneArg												#isblankFunc
	| 'SUM' anyArg													#sumFunc
	| 'PROD' anyArg													#prodFunc
	;

oneArg
	: '(' exp ')'
	;

twoArg
	: '(' exp1=exp ',' exp2=exp ')'
	;

threeArg
	: '(' exp1=exp ',' exp2=exp ',' exp3=exp ')'
	;

anyArg
	: '(' expr+=exp (',' expr+=exp)* ')'							#anyArgBase
	| '(' left=celladress ':' right=celladress ')'								#anyArgSeq
	;

celladress
	: row=CELLROW column=CELLCOLUMN											#baseAdress
	| '$' row=CELLROW column=CELLCOLUMN										#rowLockAdress
	| row=CELLROW '$' column=CELLCOLUMN										#columnLockAdress
	| '$' row=CELLROW '$' column=CELLCOLUMN									#bothLockAdress
	;

value
	: 'True'														#trueVal
	| 'False'														#falseVal
	| CHAR															#charVal
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

CELLROW
	: [A-Z]+
	;

CELLCOLUMN
	: NONFRACTPOSNUMBER+
	;

CHAR
	: '\''[a-zA-Z]'\''
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
	: NUMBER '$'
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