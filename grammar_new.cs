// --------------------------------------Программа ---------------------------------------//
// Program = Elements

// Elements = { Element }

// Element = Statement
//				| FunctionDeclaration

// FunctionDeclaration = function Identifier "(" [FormalParameters] ")" "{" FunctionBody "}"
// FormalParameters = Identifier [ , FormalParameters ]
// FunctionBody = Elements

// --------------------------------------Инструкции ---------------------------------------//
// Statement = Block 
//				| VariableStatement
//				| EmptyStatement
//				| ExpressionStatement 
//				| IfStatement 
//				| IterationStatement 
//				| BreakStatement 
//				| ReturnStatement |
//              | SwitchStatement 

// Block = "{" Statements "}"

// Statements = { Statement }

// VariableStatement = var VariableDeclarations ";"
// VariableDeclarations = VariableDeclaration { "," VariableDeclaration }
// VariableDeclaration = Identifier [ Initialiser ]
// Initialiser = "=" AssignmentExpression

// EmptyStatement = ;

// ExpressionStatement = (не начинается с {, function ) Expression ;

// IfStatement = if "(" Expression ")" Statement [ else Statement ] 

// IterationStatement = do Statement while "(" Expression ")" ";" 
//						| while "(" Expression ")" Statement
//                      | for "(" ( var VariableDeclarations | [Expression]) ";" [Expression] ";" [Expression] ) Statement 

// ContinueStatement = continue ";"

// BreakStatement = break ";" 

// ReturnStatement = return ";"

// SwitchStatement = switch "(" Expression ")" CaseBlock
// CaseBlock = "{" [CaseClauses] [ DefaultClause [CaseClauses] ] "}" 
// CaseClauses = CaseClause { CaseClause }
// CaseClause = case Expression ":" Statements
// DefaultClause = default ":" [Statements]

// --------------------------------------Выражения ---------------------------------------//
// PrimaryExpression = this 
//						| Identifier 
//						| Literal
//						| ObjectLiteral
//						| "(" Expression ")"

// Literal = NullLiteral 
//				| BooleanLiteral 
//				| StringLiteral 
//				| Number

// ObjectLiteral = "{" [PropertyNamesAndValues] "}"
// PropertyNamesAndValues = { PropertyNamesAndValues "," } PropertyAssignment 
// PropertyAssignment = PropertyName ":" AssignmentExpression | get PropertyName "(" ")" "{" FunctionBody "}" | 
// PropertyName = IdentifierName | StringLiteral

// MemberExpression = PrimaryExpression [ ( "." MemberExpression | "[" Expression "]" | "(" [ArgumentList] ")") ]		
// ArgumentList = AssignmentExpression [ "," ArgumentList ]

// ConstructorCallExpression = Identifier [ ( "(" [ArgumentList] ")" | "." ConstructorCallExpression ) ]

// ConstructorExpression = this "." ConstructorCallExpression
//					| ConstructorCallExpression
// ConstructorExpression = [this "."] ConstructorCallExpression

// UnaryExpression = MemberExpression [( "++" | "--" )]
//						| ( "+" | "-" ) UnaryExpression
//						| ( "++" | "--" ) MemberExpression
//						| new ConstructorExpression
//						| delete MemberExpression

// MultiplicativeExpression = UnaryExpression [ ( "*" | "/" | "%" ) MultiplicativeExpression ]

// AdditiveExpression = MultiplicativeExpression [ ( "+" | "-" ) AdditiveExpression ]

// ShiftExpression = AdditiveExpression [ ( "<<" | ">>" ) ShiftExpression ]

// RelationalExpression = ShiftExpression [ ( "<" | ">" | "<=" | ">=" ) RelationalExpression ] 

//// RelationalExpression = ShiftExpression RelationalExpression' 
//// RelationalExpression' = ( "<" | ">" | "<=" | ">=" ) ShiftExpression [ RelationalExpression' ]

// EqualityExpression = RelationalExpression [ ( "==" | "!=" ) EqualityExpression ]

// BitwiseAndExpression = EqualityExpression [ "&" BitwiseAndExpression ]

// BitwiseXorExpression = BitwiseAndExpression [ "^" BitwiseXorExpression ]

// BitwiseOrExpression = BitwiseXorExpression [ "|"  BitwiseOrExpression ]

// LogicalAndExpression = BitwiseOrExpression [ "&&" LogicalAndExpression ]

// LogicalOrExpression = LogicalAndExpression [ "||" LogicalOrExpression ]

// ConditionalExpression = LogicalOrExpression [ "?" AssignmentExpression ":" AssignmentExpression ]

// AssignmentExpression = ConditionalExpression [ "=" AssignmentExpression ] 

// Expression = AssignmentExpression [ "," Expression ]