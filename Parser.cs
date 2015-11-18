using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using AST;

namespace JavaScriptInterpreter
{
    partial class Parser
    {
        private Queue<Token> lexems;
        private Token sym;
        public Parser(Queue<Token> lexems)
        {
            this.lexems = lexems;
            this.sym = nextToken();
        }
        private Token nextToken()
        {
            if (lexems.Count != 0)
            {
                return lexems.Dequeue();
            }
            else
            {
                return new SpecToken(DomainTag.END_OF_PROGRAM, sym.Coords.Starting, sym.Coords.Following);
            }
        }
        private bool checkTokenTag(DomainTag tag)
        {
            return sym.Tag == tag;
        }
        private bool checkReservedWord(string reservedWord)
        {
            return checkTokenTag(DomainTag.RESERVED_WORD) && ((ReservedWordToken)sym).ReservedWord.Equals(reservedWord);
        }
        private Node parseToken(DomainTag tag)
        {
            Node node = new Node(sym);
            if (checkTokenTag(tag))
            {
                sym = nextToken();
            }
            else
            {
                JSInterpreter.ShowErrorAndStop(sym.Coords.Starting, System.String.Format("Expected \"{0}\"", tag.ToString()));
            }
            return node;
        }
        private Node parseReservedWord(string reservedWord)
        {
            Node node = new Node(sym);
            if (checkReservedWord(reservedWord))
            {
                sym = nextToken();
            }
            else
            {
                JSInterpreter.ShowErrorAndStop(sym.Coords.Starting, System.String.Format("Expected \"{0}\"", reservedWord));
            }
            return node;
        }
        public void Start()
        {
            Node parseTree;
            //if (filePath != null) File.WriteAllText(filePath, string.Empty);
            //    print("", true, filePath);
            parseTree = parseProgram();
            string filePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "parseTree.txt");
            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(parseTree.ToString());
            }
            //string filePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "parseTree.txt");
            //parseTree.print(filePath);
        }
        // --------------------------------------Программа ---------------------------------------//
        // Program = Elements
        private Node parseProgram()
        {
            Node parseTree = new Node("program");
            List<Node> elements = parseElements();
            parseTree.AddChildren(elements);
            return parseTree;
        }
        // Elements = { Element }
        private List<Node> parseElements()
        {
            List<Node> elements = new List<Node>();
            while (inFirstOfStatement() || checkReservedWord("function")) 
            {
                Node element = parseElement();
                elements.Add(element);
            }
            return elements;
        }
        // Element = Statement
        //			    | FunctionDeclaration
        private Node parseElement()
        {
            Node element;
            if (inFirstOfStatement())
            {
                element = parseStatement();
            }
            else
            {
                element = parseFunctionDeclaration();
            }
            return element;
        }
        // Объявление функции
        // FunctionDeclaration = function Identifier "(" [FormalParameters] ")" "{" FunctionBody "}"
        private Node parseFunctionDeclaration()
        {
            Node functionDeclaration = new Node("Function declaration");
            functionDeclaration.AddChild(parseReservedWord("function"));
            functionDeclaration.AddChild(parseToken(DomainTag.IDENT));
            functionDeclaration.AddChild(parseToken(DomainTag.LPARENT));
            if (checkTokenTag(DomainTag.IDENT))
            {
                functionDeclaration.AddChildren(parseFormalParameters());
            }
            functionDeclaration.AddChild(parseToken(DomainTag.RPARENT));
            functionDeclaration.AddChild(parseToken(DomainTag.LBRACE));
            functionDeclaration.AddChild(parseFunctionBody());
            functionDeclaration.AddChild(parseToken(DomainTag.RBRACE));
            return functionDeclaration;
        }
        // FormalParameters = Identifier [ , FormalParameters ]
        private List<Node> parseFormalParameters()
        {
            List<Node> formalParameters = new List<Node>();
            formalParameters.Add(parseToken(DomainTag.IDENT));
            while (checkTokenTag(DomainTag.COMMA))
            {
                formalParameters.Add(parseToken(DomainTag.COMMA));
                formalParameters.Add(parseToken(DomainTag.IDENT));
            }
            return formalParameters;
        }
        // FunctionBody = Elements
        private Node parseFunctionBody()
        {
            Node functionBody = new Node("Function body");
            functionBody.AddChildren(parseElements());
            return functionBody;
        }
        // --------------------------------------Инструкции ---------------------------------------//
        // Statement = Block | VariableStatement | EmptyStatement | ExpressionStatement | IfStatement | IterationStatement | ContinueStatement | BreakStatement | ReturnStatement |
        //                   | WithStatement | LabelledStatement | SwitchStatement | ThrowStatement | TryStatement | DebuggerStatement 
        // Statement = Block 
        //				| VariableStatement
        //				| EmptyStatement
        //				| ExpressionStatement 
        //				| IfStatement 
        //				| IterationStatement 
        //				| BreakStatement
        //				| ReturnStatement
        //              | SwitchStatement 
        //				| ContinueStatement
        private Node parseStatement()
        {
            Node statement;
            if (checkTokenTag(DomainTag.LBRACE))
            {
                statement = parseBlock();
            }
            else if (checkReservedWord("var"))
            {
                statement = parseVariableStatement();
            }
            else if (checkTokenTag(DomainTag.SEMICOLON))
            {
                statement = parseEmptyStatement();
            }
            else if (inFirstOfExpressionStatement()) // sym in first(ExpressionStatement)
            {
                statement = parseExpressionStatement();
            }
            else if (checkReservedWord("if"))
            {
                statement = parseIfStatement();
            }
            else if (checkReservedWord("for") || checkReservedWord("while") || checkReservedWord("do"))
            {
                statement = parseIterationStatement();
            }
            else if (checkReservedWord("break"))
            {
                statement = parseBreakStatement();
            }
            else if (checkReservedWord("continue"))
            {
                statement = parseContinueStatement();
            }
            else if (checkReservedWord("return"))
            {
                statement = parseReturnStatement();
            }
            else //if (checkReservedWord("switch"))
            {
                statement = parseSwitchStatement();
            }
            return statement;
        }
        // Блоки
        // Block = "{" Statements "}"
        private Node parseBlock()
        {
            Node block = new Node("Block");
            block.AddChild(parseToken(DomainTag.LBRACE));
            block.AddChildren(parseStatements());
            block.AddChild(parseToken(DomainTag.RBRACE));
            return block;
        }
        // Statements = Statement | Statements Statement
        // UPDATE: // Statements = Statement { Statement }
        // UPDATE2:  Statements = { Statement }
        private List<Node> parseStatements()
        {
            List<Node> statements = new List<Node>();
            Node statement;
            while (inFirstOfStatement())
            {
                statement = parseStatement();
                statements.Add(statement);
            }
            return statements;
        }
        // Объявление переменной
        // VariableStatement = var VariableDeclarations ";"
        private Node parseVariableStatement()
        {
            Node variableStatement = new Node("Variable statement");
            variableStatement.AddChild(parseReservedWord("var"));
            variableStatement.AddChildren(parseVariableDeclarations());
            variableStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            return variableStatement;
        }

        // VariableDeclarations = VariableDeclaration { "," VariableDeclaration }
        private List<Node> parseVariableDeclarations()
        {
            List<Node> variableDeclarations = new List<Node>();
            variableDeclarations.Add(parseVariableDeclartion());
            while (checkTokenTag(DomainTag.COMMA))
            {
                variableDeclarations.Add(parseToken(DomainTag.COMMA));
                variableDeclarations.Add(parseVariableDeclartion());
            }
            return variableDeclarations;
        }
        // VariableDeclaration = Identifier [ Initialiser ]
        private Node parseVariableDeclartion()
        {
            Node variableDeclaration = new Node("Variable declaration");
            variableDeclaration.AddChild(parseToken(DomainTag.IDENT));

            if (checkTokenTag(DomainTag.EQUAL))   // Sym in first(Initialiser)
            {
                variableDeclaration.AddChild(parseInitialiser());
            }
            return variableDeclaration;
        }
        // Initialiser = "=" AssignmentExpression
        private Node parseInitialiser()
        {
            Node initialiser = new Node("Initialiser");
            initialiser.AddChild(parseToken(DomainTag.EQUAL));
            initialiser.AddChild(parseAssignmentExpression());
            return initialiser;
        }

        // Пустая строка
        // EmptyStatement = ;
        private Node parseEmptyStatement()
        {
            Node emptyStatement = new Node("Empty statement");
            emptyStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            return emptyStatement;
        }

        // Инструкция выражение
        // ExpressionStatement = (не начинается с {, function ) Expression ;
        private Node parseExpressionStatement()
        {
            Node expressionStatement = new Node("Expression statement");
            if (!checkTokenTag(DomainTag.LBRACE) && !checkReservedWord("function"))
            {
                expressionStatement.AddChild(parseExpression());
            }
            expressionStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            return expressionStatement;
        }

        // Инструкция if
        // IfStatement = if "(" Expression ")" Statement [ else Statement ] 
        private Node parseIfStatement()
        {
            Node ifStatement = new Node("IF statement");
            ifStatement.AddChild(parseReservedWord("if"));
            ifStatement.AddChild(parseToken(DomainTag.LPARENT));
            ifStatement.AddChild(parseExpression());
            ifStatement.AddChild(parseToken(DomainTag.RPARENT));
            ifStatement.AddChild(parseStatement());
            if (checkReservedWord("else"))
            {
                ifStatement.AddChild(parseReservedWord("else"));
                ifStatement.AddChild(parseStatement());
            }
            return ifStatement;
        }

        // Инструкция циклы
        // IterationStatement = do Statement while "(" Expression ")" ; | while ( Expression ) Statement
        //                      | for ( [ExpressionNoIn] ; [Expression] ; [Expression] ) Statement
        //                      | for ( var VariableDeclarationsNoIn ; [Expression] ; [Expression] ) Statement 
        //                      | for ( LeftHandSideExpression in Expression ) Statement
        //                      | for ( var VariableDeclarationNon in Expression ) Statement

        // QUESTION: проконсультироваться
        // IterationStatement = do Statement while "(" Expression ")" ";" | while "(" Expression ")" Statement
        //                      | for "(" [Expression] ";" [Expression] ";" [Expression] ")" Statement
        //                      | for "(" var VariableDeclarations ";" [Expression] ";" [Expression] ) Statement 
        // IterationStatement = do Statement while "(" Expression ")" ";" | while "(" Expression ")" Statement
        //                      | for "(" ( var VariableDeclarations | [Expression]) ";" [Expression] ";" [Expression] ) Statement 
        // IterationStatement = do Statement while "(" Expression ")" ";" 
        //						| while "(" Expression ")" Statement
        //                      | for "(" ( var VariableDeclarations | [Expression]) ";" [Expression] ";" [Expression] ) Statement 
        private Node parseIterationStatement()
        {
            Node iterationStatement = new Node("Iteration statement");
            if (checkReservedWord("do"))
            {
                iterationStatement.AddChild(parseReservedWord("do"));
                iterationStatement.AddChild(parseStatement());
                iterationStatement.AddChild(parseReservedWord("while"));
                iterationStatement.AddChild(parseToken(DomainTag.LPARENT));
                iterationStatement.AddChild(parseExpression());
                iterationStatement.AddChild(parseToken(DomainTag.RPARENT));
                iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            }
            else if (checkReservedWord("while"))
            {
                iterationStatement.AddChild(parseReservedWord("while"));
                iterationStatement.AddChild(parseToken(DomainTag.LPARENT));
                iterationStatement.AddChild(parseExpression());
                iterationStatement.AddChild(parseToken(DomainTag.RPARENT));
                iterationStatement.AddChild(parseStatement());
            }
            else if (checkReservedWord("for"))
            {
                iterationStatement.AddChild(parseReservedWord("for"));
                iterationStatement.AddChild(parseToken(DomainTag.LPARENT));
                if (checkReservedWord("var"))
                {
                    iterationStatement.AddChild(parseReservedWord("var"));
                    // iterationStatement.AddChild(parseVariableStatement());
                    iterationStatement.AddChildren(parseVariableDeclarations());
                }
                else
                {
                    if (inFirstOfExpression())  // sym in first(expression)
                    {
                        iterationStatement.AddChild(parseExpression());
                    }

                }
                iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
                if (inFirstOfExpression())  // sym in first(expression)
                {
                    iterationStatement.AddChild(parseExpression());
                }
                iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
                if (inFirstOfExpression())  // sym in first(expression)
                {
                    iterationStatement.AddChild(parseExpression());
                }
                iterationStatement.AddChild(parseToken(DomainTag.RPARENT));
                iterationStatement.AddChild(parseStatement());
            }
            return iterationStatement;
        }
        // Инструкция continue 
        // ContinueStatement = continue ";" | continue ( без перевода строки ) Identifier ; 
        // ContinueStatement = continue ";"
        private Node parseContinueStatement()
        {
            Node continueStatement = new Node("Continue statement");
            continueStatement.AddChild(parseReservedWord("continue"));
            continueStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            // TODO/QUESTION
            // continue ( без перевода строки ) Identifier ; 
            return continueStatement;
        }
        // Инструкция break
        // BreakStatement = break ";" | break ( без перевода строки ) Identifier ;
        // BreakStatement = break ";"
        private Node parseBreakStatement()
        {
            Node breakStatement = new Node("Break statement");
            breakStatement.AddChild(parseReservedWord("break"));
            breakStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            // TODO/QUESTION
            // break ( без перевода строки ) Identifier ;
            return breakStatement;
        }

        // Инструкция return 
        // ReturnStatement = return ";" | return ( без перевода строки ) Expression;
        // ReturnStatement = return ";" |
        private Node parseReturnStatement()
        {
            Node returnStatement = new Node("Return statement");
            returnStatement.AddChild(parseReservedWord("return"));
            returnStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            // TODO!!!!/QUESTION
            // return ( без перевода строки ) Expression;
            return returnStatement;
        }
        
        // Инструкция switch
        // SwitchStatement = switch "(" Expression ")" CaseBlock
        private Node parseSwitchStatement()
        {
            Node switchStatement = new Node("Switch statement");
            switchStatement.AddChild(parseReservedWord("switch"));
            switchStatement.AddChild(parseToken(DomainTag.LPARENT));
            switchStatement.AddChild(parseExpression());
            switchStatement.AddChild(parseToken(DomainTag.RPARENT));
            switchStatement.AddChild(parseCaseBlock());
            return switchStatement;
        }
        // CaseBlock = "{" [CaseClauses] "}" | "{" [CaseClauses] DefaultClause [CaseClauses] "}"
        // CaseBlock = "{" [CaseClauses] [ DefaultClause [CaseClauses] ] "}" 
        private Node parseCaseBlock()
        {
            Node caseBlock = new Node("Case block");
            caseBlock.AddChild(parseToken(DomainTag.LBRACE));
            if (checkReservedWord("case"))
            {
                caseBlock.AddChildren(parseCaseClauses());
            }
            if (checkReservedWord("default"))
            {
                caseBlock.AddChild(parseDefaultClause());
                if (checkReservedWord("case"))
                {
                    caseBlock.AddChildren(parseCaseClauses());
                }
            }
            caseBlock.AddChild(parseToken(DomainTag.RBRACE));
            return caseBlock;
        }
        // CaseClauses = CaseClause | CaseClauses CaseClause
        // QUESTION CaseClauses = CaseClause { CaseClause }
        // CaseClausesOpt = { CaseClause }  // TODO
        private List<Node> parseCaseClauses()
        {
            List<Node> caseClauses = new List<Node>();
            Node caseClause;
            do
            {
                caseClause = parseCaseClause();
                caseClauses.Add(caseClause);
            }
            while (checkReservedWord("case"));
            return caseClauses;
        }
        // CaseClause = case Expression ":" Statements
        private Node parseCaseClause()
        {
            Node caseClause = new Node("Case clause");
            caseClause.AddChild(parseReservedWord("case"));
            caseClause.AddChild(parseExpression());
            caseClause.AddChild(parseToken(DomainTag.COLON));
            caseClause.AddChildren(parseStatements());
            return caseClause;
        }
        // DefaultClause = default ":" [Statements]
        private Node parseDefaultClause()
        {
            Node defaultClause = new Node("Default clause");
            defaultClause.AddChild(parseReservedWord("default"));
            defaultClause.AddChild(parseToken(DomainTag.COLON));
            if (inFirstOfStatement()) 
            {
                defaultClause.AddChildren(parseStatements());
            }
            return defaultClause;
        }


        // FunctionExpression = function [Identifier] "(" [FormalParameters] ")" "{" FunctionBody "}"
        private Node parseFunctionExpression()
        {
            Node functionExpression = new Node("Function expression");
            functionExpression.AddChild(parseReservedWord("function"));
            if (checkTokenTag(DomainTag.IDENT))
            {
                functionExpression.AddChild(parseToken(DomainTag.IDENT));
            }
            functionExpression.AddChild(parseToken(DomainTag.LPARENT));
            if (checkTokenTag(DomainTag.IDENT)) 
            {
                functionExpression.AddChildren(parseFormalParameters());
            }
            functionExpression.AddChild(parseToken(DomainTag.RPARENT));
            functionExpression.AddChild(parseToken(DomainTag.LBRACE));
            functionExpression.AddChild(parseFunctionBody());
            functionExpression.AddChild(parseToken(DomainTag.RBRACE));
            return functionExpression;
        }
         //FormalParameters = Identifier | FormalParameters , Identifier
         //QUESTION? FormalParameters = Identifier { , Identifier }
        

        // --------------------------------------Выражения ---------------------------------------//
        // Первичные выражения
        // PrimaryExpression = this | Identifier | Literal | ArrayLiteral | ObjectLiteral | "(" Expression ")"
        // PrimaryExpression = this 
        //						| Identifier 
        //						| Literal
        //						| ObjectLiteral
        //						| "(" Expression ")"
        private Node parsePrimaryExpression()
        {
            Node primaryExpression = new Node("Primary expression");
            if (checkReservedWord("this"))
            {
                primaryExpression.AddChild(parseReservedWord("this"));
            }
            else if (checkTokenTag(DomainTag.IDENT))
            {
                primaryExpression.AddChild(parseToken(DomainTag.IDENT));
            }
            else if (inFirstOfLiteral())
            {
                primaryExpression.AddChild(parseLiteral());
            }
            else if (inFirstOfObjectLiteral())
            {
                primaryExpression.AddChild(parseObjectLiteral());
            }
            else// if (checkTokenTag(DomainTag.LPARENT)){
            {
                primaryExpression.AddChild(parseToken(DomainTag.LPARENT));
                primaryExpression.AddChild(parseExpression());
                primaryExpression.AddChild(parseToken(DomainTag.RPARENT));
            }
            return primaryExpression;
        }
        // Literal = NullLiteral 
        //				| BooleanLiteral 
        //				| StringLiteral 
        //				| Number
        private Node parseLiteral()
        {
            //Node literal = new Node("Literal");
            Node literal;
            if (checkReservedWord("null"))
            {
                literal = parseReservedWord("null");
            }
            else if (checkReservedWord("true"))
            {
                literal = parseReservedWord("true");
            }
            else if (checkReservedWord("false"))
            {
                literal = parseReservedWord("false");
            }
            else if (checkTokenTag(DomainTag.NUMBER))
            {
               literal = parseToken(DomainTag.NUMBER);
            }
            else
            {
                literal = parseToken(DomainTag.STRING);
            }
            return literal;
        }

        // Инциализация объекта
        // ObjectLiteral = "{" "}" | "{" PropertyNamesAndValues "}"
        // ObjectLiteral = "{" [PropertyNamesAndValues] "}"
        private Node parseObjectLiteral()
        {
            Node objectLiteral = new Node("Object literal");
            objectLiteral.AddChild(parseToken(DomainTag.LBRACE));
            if (inFirstOfPropertyNamesAndValues()) // sym in first(propertyNamesAndValues)
            {
                objectLiteral.AddChildren(parsePropertyNamesAndValues());
            }
            objectLiteral.AddChild(parseToken(DomainTag.RBRACE));
            return objectLiteral;
        }
        // PropertyNamesAndValues = { PropertyNamesAndValues "," } PropertyAssignment 
        private List<Node> parsePropertyNamesAndValues()
        {
            List<Node> propertyNamesAndValues = new List<Node>();
            while (inFirstOfPropertyNamesAndValues()) // sym in first(propertyNamesAndValues)
            {
                propertyNamesAndValues.AddRange(parsePropertyNamesAndValues());
                propertyNamesAndValues.Add(parseToken(DomainTag.COMMA));
            }
            propertyNamesAndValues.Add(parsePropertyAssignment());
            return propertyNamesAndValues;
        }
        
        // PropertyAssignment = PropertyName ":" AssignmentExpression | get PropertyName "(" ")" "{" FunctionBody "}" | 
        //                          | set PropertyName "(" PropertySetParameters ")" "{" FunctionBody "}".
        private Node parsePropertyAssignment()
        {
            Node propertyAssignment = new Node("Property assignment");
            if (inFirstOfProperyName()) // First(Propertyname)
            {
                propertyAssignment.AddChild(parsePropertyName());
                propertyAssignment.AddChild(parseToken(DomainTag.COLON));
                propertyAssignment.AddChild(parseAssignmentExpression());
            }
            else if (checkReservedWord("get"))
            {
                propertyAssignment.AddChild(parseReservedWord("get"));
                propertyAssignment.AddChild(parsePropertyName());
                propertyAssignment.AddChild(parseToken(DomainTag.LPARENT));
                propertyAssignment.AddChild(parseToken(DomainTag.RPARENT));
                propertyAssignment.AddChild(parseToken(DomainTag.LBRACE));
                propertyAssignment.AddChild(parseFunctionBody());
                propertyAssignment.AddChild(parseToken(DomainTag.RBRACE));
            }
            else
            {
                propertyAssignment.AddChild(parseReservedWord("set"));
                propertyAssignment.AddChild(parsePropertyName());
                propertyAssignment.AddChild(parseToken(DomainTag.LPARENT));
                propertyAssignment.AddChildren(parsePropertySetParameters());
                propertyAssignment.AddChild(parseToken(DomainTag.RPARENT));
                propertyAssignment.AddChild(parseToken(DomainTag.LBRACE));
                propertyAssignment.AddChild(parseFunctionBody());
                propertyAssignment.AddChild(parseToken(DomainTag.RBRACE));
            }
            return propertyAssignment;
        }

        // QUESTION = NumericLiteral?
        // PropertyName = IdentifierName | StringLiteral | NumericLiteral
        // PropertyName = IdentifierName | StringLiteral
        private Node parsePropertyName()
        {
            Node propertyName;
            if (checkTokenTag(DomainTag.IDENT))
            {
                propertyName = parseToken(DomainTag.IDENT);
            }
            else
            {
                propertyName = parseToken(DomainTag.STRING);
            }
            return propertyName;
        }
        private List<Node> parsePropertySetParameters()
        {
            List<Node> propertySetParameters = new List<Node>();
            // TODO IN FUTURE
            return propertySetParameters;
        }


        // Левосторонние выражения
        // MemberExpression = PrimaryExpression | FunctionExpression | MemberExpression "[" Expression "]" 
        //                      | MemberExpression "." Identifier | new MemberExpression Arguments
        // MemberExpression = ( PrimaryExpression | FunctionExpression | new MemberExpression Arguments ) MemberExpression'
        // MemberExpression = PrimaryExpression [ ( "." MemberExpression | "[" Expression "]" | "(" [ArgumentList] ")") ]	
	    //                      | FunctionExpression
        private Node parseMemberExpression()
        {
            Node memberExpression = new Node("Member expression");
            if (inFirstOfPrimaryExpression())
            {
                memberExpression.AddChild(parsePrimaryExpression());
                if (checkTokenTag(DomainTag.POINT))
                {
                    memberExpression.AddChild(parseToken(DomainTag.POINT));
                    memberExpression.AddChild(parseMemberExpression());
                }
                else if (checkTokenTag(DomainTag.LSBRACKET))
                {
                    memberExpression.AddChild(parseToken(DomainTag.LSBRACKET));
                    memberExpression.AddChild(parseExpression());
                    memberExpression.AddChild(parseToken(DomainTag.RSBRACKET));
                }
                else if (checkTokenTag(DomainTag.LPARENT))
                {
                    memberExpression.AddChild(parseToken(DomainTag.LPARENT));
                    if (inFirstOfAssignmentExpression())
                    {
                        memberExpression.AddChildren(parseArgumentList());
                    }
                    memberExpression.AddChild(parseToken(DomainTag.RPARENT));
                }
            }
            else
            {
                memberExpression.AddChild(parseFunctionDeclaration());
            }
            return memberExpression;

        }
        // ArgumentList = AssignmentExpression [ "," ArgumentList ]
        private List<Node> parseArgumentList()
        {
            List<Node> argumentList = new List<Node>();
            argumentList.Add(parseAssignmentExpression());
            if (checkTokenTag(DomainTag.COMMA))
            {
                argumentList.Add(parseToken(DomainTag.COMMA));
                argumentList.AddRange(parseArgumentList());
            }
            return argumentList;
        }
        // ConstructorCallExpression = Identifier [ ( "(" [ArgumentList] ")" | "." ConstructorCallExpression ) ]
        private List<Node> parseConstructorCallExpression()
        {
            List<Node> constructorCall = new List<Node>();
            constructorCall.Add(parseToken(DomainTag.IDENT));
            if (checkTokenTag(DomainTag.LPARENT))
            {
                constructorCall.Add(parseToken(DomainTag.LPARENT));
                if (inFirstOfArgumentList())
                {
                    constructorCall.AddRange(parseArgumentList());
                }
                constructorCall.Add(parseToken(DomainTag.RPARENT));
            }
            else if (checkTokenTag(DomainTag.POINT))
            {
                constructorCall.Add(parseToken(DomainTag.POINT));
                constructorCall.AddRange(parseConstructorCallExpression());
            }
            return constructorCall;
        }
        // ConstructorExpression = [this "."] ConstructorCallExpression
        private List<Node> parseConstructorExpression()
        {
            List<Node> constructorExpression = new List<Node>();
            if (checkReservedWord("this"))
            {
                constructorExpression.Add(parseReservedWord("this"));
                constructorExpression.Add(parseToken(DomainTag.COMMA));
            }
            constructorExpression.AddRange(parseConstructorCallExpression());
            return constructorExpression;
        }

        // Унарные операторы
        // UnaryExpression = PostfixExpression | delete UnaryExpression | void UnaryExpression | typeof UnarryExpression |
        //                      | "++" UnaryExpression | "--" UnaryExpression | "+" UnaryExpression | "-" UnaryExpression
        //                      | "~" UnaryExpresssion | "!" UnaryExpression 
        // UnaryExpression = PostfixExpression | delete UnaryExpression | void UnaryExpression | typeof UnarryExpression |
        //                      | "++" UnaryExpression | "--" UnaryExpression | "+" UnaryExpression | "-" UnaryExpression

        // UnaryExpression = MemberExpression [( "++" | "--" )]
        //						| ( "+" | "-" ) UnaryExpression
        //						| ( "++" | "--" ) MemberExpression
        //						| new ConstructorExpression
        //						| delete MemberExpression
        private Node parseUnaryExpression()
        {
            Node unaryExpression = new Node("Unary expression");
            if (inFirstOfMemberExpression())
            {
                unaryExpression.AddChild(parseMemberExpression());
                if (checkTokenTag(DomainTag.INCREMENT))
                {
                    unaryExpression.AddChild(parseToken(DomainTag.INCREMENT));
                }
                else if (checkTokenTag(DomainTag.DECREMENT))
                {
                    unaryExpression.AddChild(parseToken(DomainTag.DECREMENT));
                }
            }
            else if (checkTokenTag(DomainTag.PLUS) || checkTokenTag(DomainTag.MINUS))
            {
                if (checkTokenTag(DomainTag.PLUS))
                {
                    unaryExpression.AddChild(parseToken(DomainTag.PLUS));
                }
                else
                {
                    unaryExpression.AddChild(parseToken(DomainTag.MINUS));
                }
                unaryExpression.AddChild(parseUnaryExpression());
            }
            else if (checkTokenTag(DomainTag.INCREMENT) || checkTokenTag(DomainTag.DECREMENT))
            {
                if (checkTokenTag(DomainTag.INCREMENT))
                {
                    unaryExpression.AddChild(parseToken(DomainTag.INCREMENT));
                }
                else
                {
                    unaryExpression.AddChild(parseToken(DomainTag.DECREMENT));
                }
                unaryExpression.AddChild(parseMemberExpression());
            }
            else if (checkReservedWord("new"))
            {
                unaryExpression.AddChild(parseReservedWord("new"));
                unaryExpression.AddChildren(parseConstructorExpression());
            }
            else
            {
                unaryExpression.AddChild(parseReservedWord("delete"));
                unaryExpression.AddChild(parseMemberExpression());
            }
            return unaryExpression;
        }
        // Мультипликативные
        // MultiplicativeExpression = UnaryExpression | MultiplicativeExpression "*" UnaryExpression |
        //                              | MultiplicativeExpression "/" UnaryExpression | MultiplicativeExpression "%" UnaryExpression

        // MultiplicativeExpression = UnaryExpression MultiplicativeExpression'
        // MultiplicativeExpression = UnaryExpression [ ( "*" | "/" | "%" ) MultiplicativeExpression ]
        private Node parseMultiplicativeExpression()
        {
            Node multiplicativeExpression = new Node("Multiplicative expression");
            multiplicativeExpression.AddChild(parseUnaryExpression());
            if (checkTokenTag(DomainTag.MUL) || checkTokenTag(DomainTag.DIV) || checkTokenTag(DomainTag.PERCENT))
            {
                if (checkTokenTag(DomainTag.MUL))
                {
                    multiplicativeExpression.AddChild(parseToken(DomainTag.MUL));
                }
                else if (checkTokenTag(DomainTag.DIV))
                {
                    multiplicativeExpression.AddChild(parseToken(DomainTag.DIV));
                }
                else
                {
                    multiplicativeExpression.AddChild(parseToken(DomainTag.PERCENT));
                }
                multiplicativeExpression.AddChild(parseMultiplicativeExpression());
            }
            return multiplicativeExpression;
        }
        // Аддитивные операторы
        // AdditiveExpression = MultiplicativeExpression | AdditiveExpression "+" MultiplicativeExpression |
        //                          | AdditiveExpression "-" MultiplicativeExpression

        // AdditiveExpression = MultiplicativeExpression AdditiveExpression'
        
        // AdditiveExpression = MultiplicativeExpression [ ( "+" | "-" ) AdditiveExpression ]
        private Node parseAdditiveExpression()
        {
            Node additiveExpression = new Node("Additive expression");
            additiveExpression.AddChild(parseMultiplicativeExpression());
            if (checkTokenTag(DomainTag.PLUS) || checkTokenTag(DomainTag.MINUS))
            {
                if (checkTokenTag(DomainTag.PLUS))
                {
                    additiveExpression.AddChild(parseToken(DomainTag.PLUS));
                }
                else
                {
                    additiveExpression.AddChild(parseToken(DomainTag.MINUS));
                }
                additiveExpression.AddChild(parseAdditiveExpression());
            }
            return additiveExpression;
        }
        // Операторы побитового сдвига
        // ShiftExpression = AdditiveExpression | ShiftExpression "<<" AdditiveExrpession 
        //                      | ShiftExpression ">>" AdditiveExpression

        // ShiftExpression = AdditiveExpression ShiftExpression'
        // ShiftExpression = AdditiveExpression [ ( "<<" | ">>" ) ShiftExpression ]
        private Node parseShiftExpression()
        {
            Node shiftExpression = new Node("Shift expression");
            shiftExpression.AddChild(parseAdditiveExpression());
            if (checkTokenTag(DomainTag.LSHIFT) || checkTokenTag(DomainTag.RSHIFT))
            {
                if (checkTokenTag(DomainTag.LSHIFT))
                {
                    shiftExpression.AddChild(parseToken(DomainTag.LSHIFT));
                }
                else
                {
                    shiftExpression.AddChild(parseToken(DomainTag.RSHIFT));
                }
                shiftExpression.AddChild(parseShiftExpression());
            }
            return shiftExpression;
        }
        // Операторы отношения
        // RelationalExpressionNoIn = ShiftExpression | RelationalExpressionNoIn "<" ShiftExpression 
        //                              | RelationalExpressionNoIn ">" ShiftExpression 
        //                              | RelationalExpressionNoIn "<=" ShiftExpression 
        //                              | RelationalExpressionNoIn ">=" ShiftExpression
        //                              | RelationalExpressionNoIn instanceof ShiftExpression (not implemented)
        // RelationalExpressionNoIn = ShiftExpression RelationalExpressionNoIn' 
        // RelationalExpression = ShiftExpression [ ( "<" | ">" | "<=" | ">=" ) RelationalExpression ] 
        private Node parseRelationExpression()
        {
            Node relationExpression = new Node("Relation expression");
            relationExpression.AddChild(parseShiftExpression());
            if (checkTokenTag(DomainTag.LESS) || checkTokenTag(DomainTag.LARGER)
                || checkTokenTag(DomainTag.LESS_OR_EQUAL) || checkTokenTag(DomainTag.LARGER_OR_EQUAL))
            {
                if (checkTokenTag(DomainTag.LESS)) relationExpression.AddChild(parseToken(DomainTag.LESS));
                else if (checkTokenTag(DomainTag.LARGER)) relationExpression.AddChild(parseToken(DomainTag.LARGER));
                else if (checkTokenTag(DomainTag.LESS_OR_EQUAL)) relationExpression.AddChild(parseToken(DomainTag.LESS_OR_EQUAL));
                else relationExpression.AddChild(parseToken(DomainTag.LARGER_OR_EQUAL));
                relationExpression.AddChild(parseRelationExpression());
            }
            return relationExpression;
        }
        // Операторы равенства
        //  EqualityExpressionNoIn = RelationalExpressionNoIn | EqualityExpressionNoIn "==" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "!=" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "===" RelationalExpressionNoIn (not implemented)
        //                              | EqualityExpressionNoIn "!==" RelationalExpressionNoIn (not implemented)

        //  EqualityExpressionNoIn = RelationalExpressionNoIn EqualityExpressionNoIn'
        // EqualityExpression = RelationalExpression [ ( "==" | "!=" ) EqualityExpression ]
        private Node parseEqulityExpression()
        {
            Node equlityExpression = new Node("Equlity expression");
            equlityExpression.AddChild(parseRelationExpression());
            if (checkTokenTag(DomainTag.LOGICAL_EQUAL) || checkTokenTag(DomainTag.LOGICAL_NOT_EQUAL))
            {
                if (checkTokenTag(DomainTag.LOGICAL_EQUAL))
                {
                    equlityExpression.AddChild(parseToken(DomainTag.LOGICAL_EQUAL));
                }
                else
                {
                    equlityExpression.AddChild(parseToken(DomainTag.LOGICAL_NOT_EQUAL));
                }
                equlityExpression.AddChild(parseEqulityExpression());
            }
            return equlityExpression;
        }
        // Бинарные побитовые операции
        // BitwiseANDExpressionNoIn = EqualityExpressionNoIn | BitwiseANDExpressionNoIn "&" EqualityExpressionNoIn

        // BitwiseANDExpressionNoIn = EqualityExpressionNoIn BitwiseANDExpressionNoIn'
        
        // BitwiseAndExpression = EqualityExpression [ "&" BitwiseAndExpression ]
        private Node parseBitwiseAndExpression()
        {
            Node bitwiseAndExpression = new Node("Bitwise AND expression");
            bitwiseAndExpression.AddChild(parseEqulityExpression());
            if (checkTokenTag(DomainTag.AND))
            {
                bitwiseAndExpression.AddChild(parseToken(DomainTag.AND));
                bitwiseAndExpression.AddChild(parseBitwiseAndExpression());
            }
            return bitwiseAndExpression;
        }

        // BitwiseXORExpressionNoIn = BitwiseANDExpressionNoIn | BitwiseXORExpressionNoIn "^" BitwiseANDExpressionNoIn
        // BitwiseXORExpressionNoIn = BitwiseANDExpressionNoIn BitwiseXORExpressionNoIn'

        // BitwiseXorExpression = BitwiseAndExpression [ "^" BitwiseXorExpression ]
        private Node parseBitwiseXorExpression()
        {
            Node bitwiseXorExpression = new Node("Bitwise XOR expression");
            bitwiseXorExpression.AddChild(parseBitwiseAndExpression());
            if (checkTokenTag(DomainTag.XOR))
            {
                bitwiseXorExpression.AddChild(parseToken(DomainTag.XOR));
                bitwiseXorExpression.AddChild(parseBitwiseXorExpression());
            }
            return bitwiseXorExpression;
        }
        // BitwiseORExpressionNoIn = BitwiseXORExpressionNoIn | BitwiseORExpressionNoIn "|" BitwiseXORExpressionNoIn
        // BitwiseORExpressionNoIn = BitwiseXORExpressionNoIn  BitwiseORExpressionNoIn'
        // BitwiseOrExpression = BitwiseXorExpression [ "|"  BitwiseOrExpression ]
        private Node parseBitwiseOrExpression()
        {
            Node bitwiseOrExpression = new Node("Bitwise OR expression");
            bitwiseOrExpression.AddChild(parseBitwiseXorExpression());
            if (checkTokenTag(DomainTag.OR))
            {
                bitwiseOrExpression.AddChild(parseToken(DomainTag.OR));
                bitwiseOrExpression.AddChild(parseBitwiseOrExpression());
            }
            return bitwiseOrExpression;
        }
        // Бинарные логические операторы
        // LogicalANDExpressionNoIn = BitwiseORExpressionNoIn | LogicalANDExpressionNoIn "&&" BitwiseORExpressionNoIn 
        // LogicalANDExpressionNoIn = BitwiseORExpressionNoIn LogicalANDExpressionNoIn'
        // LogicalAndExpression = BitwiseOrExpression [ "&&" LogicalAndExpression ]
        private Node parseLogicalAndExpression()
        {
            Node logicalAndExpression = new Node("Logical AND expression");
            logicalAndExpression.AddChild(parseBitwiseOrExpression());
            if (checkTokenTag(DomainTag.LOGICAL_AND))
            {
                logicalAndExpression.AddChild(parseToken(DomainTag.LOGICAL_AND));
                logicalAndExpression.AddChild(parseLogicalAndExpression());
            }
            return logicalAndExpression;
        }
        // LogicalORExpressionNoIn = LogicalANDExpressionNoIn | LogicalORExpressionNoIn "||" LogicalANDExpressionNoIn
        // LogicalORExpressionNoIn = LogicalANDExpressionNoIn LogicalORExpressionNoIn'
        // LogicalOrExpression = LogicalAndExpression [ "||" LogicalOrExpression ]
        private Node parseLogicalOrExpression()
        {
            Node logicalOrExpression = new Node("Logical OR expression");
            logicalOrExpression.AddChild(parseLogicalAndExpression());
            if (checkTokenTag(DomainTag.LOGICAL_OR))
            {
                logicalOrExpression.AddChild(parseToken(DomainTag.OR));
                logicalOrExpression.AddChild(parseLogicalOrExpression());
            }
            return logicalOrExpression;
        }
        // Условные оператор ( ? : )
        // ConditionalExpressionNoIn = LogicalORExpressionNoIn 
        //                              | LogicalORExpressionNoIn "?" AssignmentExpression ":" AssignmentExpressionNoIn
        // ConditionalExpressionNoIn = LogicalORExpressionNoIn [ "?" AssignmentExpression ":" AssignmentExpressionNoIn ]
        // ConditionalExpression = LogicalOrExpression [ "?" AssignmentExpression ":" AssignmentExpression ]
        private Node parseConditionalExpression()
        {
            Node conditionalExpression = new Node("Conditional expression");
            conditionalExpression.AddChild(parseLogicalOrExpression());
            if (checkTokenTag(DomainTag.QUESTION))
            {
                conditionalExpression.AddChild(parseToken(DomainTag.QUESTION));
                conditionalExpression.AddChild(parseAssignmentExpression());
                conditionalExpression.AddChild(parseToken(DomainTag.COLON));
                conditionalExpression.AddChild(parseAssignmentExpression());
            }
            return conditionalExpression;
        }
        // TODO
        // AssignmentExpressionNoIn = ConditionalExpressionNoIn | LeftHandSideExpression AssignmentOperator AssignmentExpressionNoIn
        // AssignmentExpression = ConditionalExpression [ "=" AssignmentExpression ] 
        private Node parseAssignmentExpression()
        {
            Node assignmentExpression = new Node("Assignment expression");
            assignmentExpression.AddChild(parseConditionalExpression());
            if (checkTokenTag(DomainTag.EQUAL))
            {
                assignmentExpression.AddChild(parseToken(DomainTag.EQUAL));
                assignmentExpression.AddChild(parseAssignmentExpression());
            }
            return assignmentExpression;
        }
        // Оператор запятая
        // ExpressionNoIn = AssignmentExpressionNoIn | ExpressionNoIn "," AssignmentExpressionNoIn
        // ExpressionNoIn = AssignmentExpressionNoIn ExpressionNoIn'
        // Expression = AssignmentExpression [ "," Expression ]
        private Node parseExpression()
        {
            Node expression = new Node("Expression");
            expression.AddChild(parseAssignmentExpression());
            if (checkTokenTag(DomainTag.COMMA))
            {
                expression.AddChild(parseToken(DomainTag.COMMA));
                expression.AddChild(parseExpression());
            }
            return expression;
        }
    }
}
