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
        public Node Start()
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
            return parseTree;
            //string filePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "parseTree.txt");
            //parseTree.print(filePath);
        }
        // --------------------------------------Программа ---------------------------------------//
        // Program = Elements
        private Node parseProgram()
        {
            Node parseTree = new Node("input");
            List<Element> elements = parseElements();
            parseTree.AddChildren(elements);
            return parseTree;
        }
        // Elements = { Element }
        private List<AST.Element> parseElements()
        {
            List<AST.Element> elements = new List<AST.Element>();
            while (inFirstOfStatement() || checkReservedWord("function")) 
            {
                AST.Element element = parseElement();
                elements.Add(element);
            }
            return elements;
        }
        // Element = Statement
        //			    | FunctionDeclaration
        private AST.Element parseElement()
        {
            AST.Element element;
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
        private List<Element> parseFormalParameters()
        {
            List<Element> formalParameters = new List<Element>();
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
        private AST.Element parseStatement()
        {
            AST.Element statement;
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
        private AST.BlockStatement parseBlock()
        {

            parseToken(DomainTag.LBRACE);
            List<AST.Element> statements = parseStatements();
            parseToken(DomainTag.RBRACE);

            return new AST.BlockStatement(statements);
        }
        // Statements = Statement | Statements Statement
        // UPDATE: // Statements = Statement { Statement }
        // UPDATE2:  Statements = { Statement }
        private List<Element> parseStatements()
        {
            List<Element> statements = new List<Element>();
            AST.Element statement;
            while (inFirstOfStatement())
            {
                statement = parseStatement();
                statements.Add(statement);
            }
            return statements;
        }
        // Объявление переменной
        // VariableStatement = var VariableDeclarations ";"
        private AST.Var parseVariableStatement()
        {
            parseReservedWord("var");
            AST.Var variableStatement = new AST.Var(parseVariableDeclarations());
            if (checkTokenTag(DomainTag.SEMICOLON))
            {
                parseToken(DomainTag.SEMICOLON);
            }
            return variableStatement;
        }

        // VariableDeclarations = VariableDeclaration { "," VariableDeclaration }
        private List<AST.VarDeclaration> parseVariableDeclarations()
        {
            List<AST.VarDeclaration> variableDeclarations = new List<AST.VarDeclaration>();
            variableDeclarations.Add(parseVariableDeclartion());
            while (checkTokenTag(DomainTag.COMMA))
            {
                parseToken(DomainTag.COMMA);
                variableDeclarations.Add(parseVariableDeclartion());
            }
            return variableDeclarations;
        }
        // VariableDeclaration = Identifier [ Initialiser ]
        private AST.VarDeclaration parseVariableDeclartion()
        {
            string name = ((IdentToken)sym).Name;
            parseToken(DomainTag.IDENT);
            AST.Identifier ident = new AST.Identifier(name);
            AST.Expression val = null;
            if (checkTokenTag(DomainTag.EQUAL))   // Sym in first(Initialiser)
            {
                val = parseInitialiser();
            }
            return new AST.VarDeclaration(ident, val);
        }
        // Initialiser = "=" AssignmentExpression
        private AST.Expression parseInitialiser()
        {
            parseToken(DomainTag.EQUAL);
            return parseAssignmentExpression();
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
        private ExpressionStatment parseExpressionStatement()
        {
            ExpressionStatment expressionStatement=null;
            if (!checkTokenTag(DomainTag.LBRACE) && !checkReservedWord("function"))
            {
                expressionStatement = new ExpressionStatment(parseExpression());
            }
            if (checkTokenTag(DomainTag.SEMICOLON))
            {
                parseToken(DomainTag.SEMICOLON);
            }
            return expressionStatement;
        }

        // Инструкция if
        // IfStatement = if "(" Expression ")" Statement [ else Statement ] 
        private AST.IfStatement parseIfStatement()
        {
            AST.Expression condition;
            AST.Statement ifTrue, ifFalse=null;
            parseReservedWord("if");
            parseToken(DomainTag.LPARENT);
            condition = parseExpression()[0];
            parseToken(DomainTag.RPARENT);
            ifTrue = (AST.Statement)parseStatement();
            if (checkReservedWord("else"))
            {
                parseReservedWord("else");
                ifFalse = (AST.Statement)parseStatement();
            }
            return new AST.IfStatement(condition, ifTrue, ifFalse);
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
        private AST.Statement parseIterationStatement()
        {
            Node iterationStatement = new Node("Iteration statement");
            if (checkReservedWord("do"))
            {
                parseReservedWord("do");
                AST.Statement toDo = (AST.Statement)parseStatement();
                parseReservedWord("while");
                parseToken(DomainTag.LPARENT);
                AST.Expression condition = parseExpression()[0];
                parseToken(DomainTag.RPARENT);
                if (checkTokenTag(DomainTag.SEMICOLON))
                {
                    parseToken(DomainTag.SEMICOLON);
                }
                return new AST.DoWhileStatement(toDo, condition);
            }
            else if (checkReservedWord("while"))
            {
                parseReservedWord("while");
                parseToken(DomainTag.LPARENT);
                AST.Expression condition = parseExpression()[0];
                parseToken(DomainTag.RPARENT);
                AST.Statement toDo = (AST.Statement)parseStatement();
                return new AST.WhileStatement(condition, toDo);
            }
            else if (checkReservedWord("for"))
            {
                iterationStatement.AddChild(parseReservedWord("for"));
                iterationStatement.AddChild(parseToken(DomainTag.LPARENT));
                if (checkReservedWord("var"))
                {
                    iterationStatement.AddChild(parseReservedWord("var"));
                    // iterationStatement.AddChild(parseVariableStatement());

                    throw new NotImplementedException();
                    // TODO: внизу раскоментировать
                    //iterationStatement.AddChildren(parseVariableDeclarations());
                }
                else
                {
                    if (inFirstOfExpression())  // sym in first(expression)
                    {
                        iterationStatement.AddChildren(parseExpression());
                    }

                }
                iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
                if (inFirstOfExpression())  // sym in first(expression)
                {
                    iterationStatement.AddChildren(parseExpression());
                }
                iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
                if (inFirstOfExpression())  // sym in first(expression)
                {
                    iterationStatement.AddChildren(parseExpression());
                }
                iterationStatement.AddChild(parseToken(DomainTag.RPARENT));
                iterationStatement.AddChild(parseStatement());
            }
            return null;
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
            switchStatement.AddChildren(parseExpression());
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
        private List<Element> parseCaseClauses()
        {
            List<Element> caseClauses = new List<Element>();
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
            caseClause.AddChildren(parseExpression());
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
        private Expression parseFunctionExpression()
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
            return null;
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
        private Expression parsePrimaryExpression()
        {
            if (checkReservedWord("this"))
            {
                parseReservedWord("this");
                return new This();
            }
            else if (checkTokenTag(DomainTag.IDENT))
            {
                string name = ((IdentToken)sym).Name;
                parseToken(DomainTag.IDENT);
                return new Identifier(name);
            }
            else if (inFirstOfLiteral())
            {
                return parseLiteral();
            }
            else if (inFirstOfObjectLiteral())
            {
                return parseObjectLiteral();
            }
            else// if (checkTokenTag(DomainTag.LPARENT)){
            {
                List<Expression> expressions;
                parseToken(DomainTag.LPARENT);
                expressions = parseExpression();
                parseToken(DomainTag.RPARENT);
                return expressions[expressions.Count - 1];
            }
        }
        // Literal = NullLiteral 
        //				| BooleanLiteral 
        //				| StringLiteral 
        //				| Number
        private Expression parseLiteral()
        {
            //Node literal = new Node("Literal");
            if (checkReservedWord("null"))
            {
                parseReservedWord("null");
                return new Null();
            }
            else if (checkReservedWord("true"))
            {
                parseReservedWord("true");
                return new AST.Boolean(true);
            }
            else if (checkReservedWord("false"))
            {
                parseReservedWord("false");
                return new AST.Boolean(false);
            }
            else if (checkTokenTag(DomainTag.NUMBER))
            {
               double value = ((NumberToken)sym).Value;
               parseToken(DomainTag.NUMBER);
               return new Number(value);
            }
            else
            {
                System.String value = ((StringToken)sym).Value;
                parseToken(DomainTag.STRING);
                return new AST.String(value);
            }
        }

        // Инциализация объекта
        // ObjectLiteral = "{" "}" | "{" PropertyNamesAndValues "}"
        // ObjectLiteral = "{" [PropertyNamesAndValues] "}"
        private AST.Object parseObjectLiteral()
        {
            AST.Object obj = new AST.Object();
            parseToken(DomainTag.LBRACE);
            if (inFirstOfPropertyNamesAndValues()) // sym in first(propertyNamesAndValues)
            {
                obj = new AST.Object(parsePropertyNamesAndValues());
            }
            parseToken(DomainTag.RBRACE);
            return obj;
        }
        // PropertyNamesAndValues = { PropertyNamesAndValues "," } PropertyAssignment 
        // PropertyNamesAndValues = PropertyAssignment { "," PropertyAssignment }
        private List<ObjectProperty> parsePropertyNamesAndValues()
        {
            List<ObjectProperty> propertyNamesAndValues = new List<ObjectProperty>();
            propertyNamesAndValues.Add(parsePropertyAssignment());
            while (checkTokenTag(DomainTag.COMMA)) // sym in first(propertyNamesAndValues)
            {
                parseToken(DomainTag.COMMA);
                propertyNamesAndValues.Add(parsePropertyAssignment());
            }
            return propertyNamesAndValues;
        }
        
        // PropertyAssignment = PropertyName ":" AssignmentExpression | get PropertyName "(" ")" "{" FunctionBody "}" | 
        //                          | set PropertyName "(" PropertySetParameters ")" "{" FunctionBody "}".
        private ObjectProperty parsePropertyAssignment()
        {
            if (inFirstOfProperyName()) // First(Propertyname)
            {
                string name = parsePropertyName();
                parseToken(DomainTag.COLON);
                Expression value = parseAssignmentExpression();
                return new ObjectProperty(name, value);
            }
            else if (checkReservedWord("get"))
            {
                throw new NotImplementedException();
                parseReservedWord("get");
                parsePropertyName();
                parseToken(DomainTag.LPARENT);
                parseToken(DomainTag.RPARENT);
                parseToken(DomainTag.LBRACE);
                parseFunctionBody();
                parseToken(DomainTag.RBRACE);
            }
            else
            {
                throw new NotImplementedException();
                parseReservedWord("set");
                parsePropertyName();
                parseToken(DomainTag.LPARENT);
                parsePropertySetParameters();
                parseToken(DomainTag.RPARENT);
                parseToken(DomainTag.LBRACE);
                parseFunctionBody();
                parseToken(DomainTag.RBRACE);
            }
        }

        // QUESTION = NumericLiteral?
        // PropertyName = IdentifierName | StringLiteral | NumericLiteral
        // PropertyName = IdentifierName | StringLiteral
        private string parsePropertyName()
        {
            if (checkTokenTag(DomainTag.IDENT))
            {
                string name = ((IdentToken)sym).Name;
                parseToken(DomainTag.IDENT);
                return name;
            }
            else
            {
                string name = ((StringToken)sym).Value;
                parseToken(DomainTag.STRING);
                return name;
            }
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
        private Expression parseMemberExpression()
        {
            if (inFirstOfPrimaryExpression())
            {
                Expression primaryExpression = parsePrimaryExpression();
                if (checkTokenTag(DomainTag.POINT))
                {
                    parseToken(DomainTag.POINT);
                    return new GetProperty(primaryExpression, parseMemberExpression());
                }
                else if (checkTokenTag(DomainTag.LSBRACKET))
                {
                    throw new NotImplementedException();
                    parseToken(DomainTag.LSBRACKET);
                    parseExpression();
                    parseToken(DomainTag.RSBRACKET);
                }
                else if (checkTokenTag(DomainTag.LPARENT))
                {
                    throw new NotImplementedException();
                    parseToken(DomainTag.LPARENT);
                    if (inFirstOfAssignmentExpression())
                    {
                        parseArgumentList();
                    }
                    parseToken(DomainTag.RPARENT);
                }
                return primaryExpression;
            }
            else
            {
                throw new NotImplementedException();
                return parseFunctionExpression();
            }
        }
        // ArgumentList = AssignmentExpression [ "," ArgumentList ]
        private List<Expression> parseArgumentList()
        {
            List<Expression> argumentList = new List<Expression>();
            argumentList.Add(parseAssignmentExpression());
            if (checkTokenTag(DomainTag.COMMA))
            {
                parseToken(DomainTag.COMMA);
                argumentList.AddRange(parseArgumentList());
            }
            return argumentList;
        }
        // ConstructorCallExpression = Identifier [ ( "(" [ArgumentList] ")" | "." ConstructorCallExpression ) ]
        private Expression parseConstructorCallExpression()
        {
            throw new NotImplementedException();
            parseToken(DomainTag.IDENT);
            if (checkTokenTag(DomainTag.LPARENT))
            {
                parseToken(DomainTag.LPARENT);
                if (inFirstOfArgumentList())
                {
                    parseArgumentList();
                }
               parseToken(DomainTag.RPARENT);
            }
            else if (checkTokenTag(DomainTag.POINT))
            {
                parseToken(DomainTag.POINT);
                parseConstructorCallExpression();
            }
            return null;
        }
        // ConstructorExpression = [this "."] ConstructorCallExpression
        private Expression parseConstructorExpression()
        {
            if (checkReservedWord("this"))
            {
                parseReservedWord("this");
                parseToken(DomainTag.COMMA);
            }
            return parseConstructorCallExpression();
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
        private Expression parseUnaryExpression()
        {
            if (inFirstOfMemberExpression())
            {
                Expression memberExpression = parseMemberExpression();
                if (checkTokenTag(DomainTag.INCREMENT) || checkTokenTag(DomainTag.DECREMENT))
                {
                    if (checkTokenTag(DomainTag.INCREMENT))
                    {
                        parseToken(DomainTag.INCREMENT);
                        return new Increment(memberExpression);
                    }
                    else //if (checkTokenTag(DomainTag.DECREMENT))
                    {
                        parseToken(DomainTag.DECREMENT);
                        return new Decrement(memberExpression);
                    }
                }
                return memberExpression;
            }
            else if (checkTokenTag(DomainTag.PLUS) || checkTokenTag(DomainTag.MINUS))
            {
                throw new NotImplementedException();
                if (checkTokenTag(DomainTag.PLUS))
                {
                    parseToken(DomainTag.PLUS);
                }
                else
                {
                    parseToken(DomainTag.MINUS);
                }
                parseUnaryExpression();
            }
            else if (checkTokenTag(DomainTag.INCREMENT) || checkTokenTag(DomainTag.DECREMENT))
            {
                throw new NotImplementedException();
                if (checkTokenTag(DomainTag.INCREMENT))
                {
                    parseToken(DomainTag.INCREMENT);
                }
                else
                {
                    parseToken(DomainTag.DECREMENT);
                }
                parseMemberExpression();
            }
            else if (checkReservedWord("new"))
            {
                throw new NotImplementedException();
                parseReservedWord("new");
                return new NewExpr(parseConstructorExpression());
            }
            else
            {
                throw new NotImplementedException();
                parseReservedWord("delete");
                return new DeleteExpr(parseMemberExpression());
            }
        }
        // Мультипликативные
        // MultiplicativeExpression = UnaryExpression | MultiplicativeExpression "*" UnaryExpression |
        //                              | MultiplicativeExpression "/" UnaryExpression | MultiplicativeExpression "%" UnaryExpression

        // MultiplicativeExpression = UnaryExpression MultiplicativeExpression'
        // MultiplicativeExpression = UnaryExpression [ ( "*" | "/" | "%" ) MultiplicativeExpression ]
        private Expression parseMultiplicativeExpression()
        {
            Expression unaryExpression = parseUnaryExpression();
            if (checkTokenTag(DomainTag.MUL) || checkTokenTag(DomainTag.DIV) || checkTokenTag(DomainTag.PERCENT))
            {
                if (checkTokenTag(DomainTag.MUL))
                {
                    parseToken(DomainTag.MUL);
                    return new Mul(unaryExpression, parseMultiplicativeExpression());
                }
                else if (checkTokenTag(DomainTag.DIV))
                {
                    parseToken(DomainTag.DIV);
                    return new Div(unaryExpression, parseMultiplicativeExpression());
                }
                else
                {
                    parseToken(DomainTag.PERCENT);
                    return new Remainder(unaryExpression, parseMultiplicativeExpression());
                }
            }
            return unaryExpression;
        }
        // Аддитивные операторы
        // AdditiveExpression = MultiplicativeExpression | AdditiveExpression "+" MultiplicativeExpression |
        //                          | AdditiveExpression "-" MultiplicativeExpression

        // AdditiveExpression = MultiplicativeExpression AdditiveExpression'
        
        // AdditiveExpression = MultiplicativeExpression [ ( "+" | "-" ) AdditiveExpression ]
        private Expression parseAdditiveExpression()
        {
            Expression multiplicativeExpression = parseMultiplicativeExpression();
            if (checkTokenTag(DomainTag.PLUS) || checkTokenTag(DomainTag.MINUS))
            {
                if (checkTokenTag(DomainTag.PLUS))
                {
                    parseToken(DomainTag.PLUS);
                    return new Plus(multiplicativeExpression, parseAdditiveExpression());
                }
                else
                {
                    parseToken(DomainTag.MINUS);
                    return new Minus(multiplicativeExpression, parseAdditiveExpression());
                }
            }
            return multiplicativeExpression;
        }
        // Операторы побитового сдвига
        // ShiftExpression = AdditiveExpression | ShiftExpression "<<" AdditiveExrpession 
        //                      | ShiftExpression ">>" AdditiveExpression

        // ShiftExpression = AdditiveExpression ShiftExpression'
        // ShiftExpression = AdditiveExpression [ ( "<<" | ">>" ) ShiftExpression ]
        private Expression parseShiftExpression()
        {
            Expression additiveExpression = parseAdditiveExpression();
            if (checkTokenTag(DomainTag.LSHIFT) || checkTokenTag(DomainTag.RSHIFT))
            {
                if (checkTokenTag(DomainTag.LSHIFT))
                {
                    parseToken(DomainTag.LSHIFT);
                    return new Lshift(additiveExpression, parseShiftExpression());
                }
                else
                {
                    parseToken(DomainTag.RSHIFT);
                    return new Rshift(additiveExpression, parseShiftExpression());
                }
            }
            return additiveExpression;
        }
        // Операторы отношения
        // RelationalExpressionNoIn = ShiftExpression | RelationalExpressionNoIn "<" ShiftExpression 
        //                              | RelationalExpressionNoIn ">" ShiftExpression 
        //                              | RelationalExpressionNoIn "<=" ShiftExpression 
        //                              | RelationalExpressionNoIn ">=" ShiftExpression
        //                              | RelationalExpressionNoIn instanceof ShiftExpression (not implemented)
        // RelationalExpressionNoIn = ShiftExpression RelationalExpressionNoIn' 
        // RelationalExpression = ShiftExpression [ ( "<" | ">" | "<=" | ">=" ) RelationalExpression ] 
        private Expression parseRelationExpression()
        {
            Expression shiftExpression = parseShiftExpression();
            if (checkTokenTag(DomainTag.LESS) || checkTokenTag(DomainTag.LARGER)
                || checkTokenTag(DomainTag.LESS_OR_EQUAL) || checkTokenTag(DomainTag.LARGER_OR_EQUAL))
            {
                if (checkTokenTag(DomainTag.LESS))
                {
                    parseToken(DomainTag.LESS);
                    return new LogicalLess(shiftExpression, parseRelationExpression());
                }
                else if (checkTokenTag(DomainTag.LARGER))
                {
                    parseToken(DomainTag.LARGER);
                    return new LogicalLarger(shiftExpression, parseRelationExpression());
                }
                else if (checkTokenTag(DomainTag.LESS_OR_EQUAL))
                {
                    parseToken(DomainTag.LESS_OR_EQUAL);
                    return new LogicalLessOrEqual(shiftExpression, parseRelationExpression());
                }
                else
                {
                    parseToken(DomainTag.LARGER_OR_EQUAL);
                    return new LogicalLargerOrEqual(shiftExpression, parseRelationExpression());
                }
            }
            return shiftExpression;
        }
        // Операторы равенства
        //  EqualityExpressionNoIn = RelationalExpressionNoIn | EqualityExpressionNoIn "==" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "!=" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "===" RelationalExpressionNoIn (not implemented)
        //                              | EqualityExpressionNoIn "!==" RelationalExpressionNoIn (not implemented)

        //  EqualityExpressionNoIn = RelationalExpressionNoIn EqualityExpressionNoIn'
        // EqualityExpression = RelationalExpression [ ( "==" | "!=" ) EqualityExpression ]
        private Expression parseEqulityExpression()
        {
            Expression relationalExpression = parseRelationExpression();
            if (checkTokenTag(DomainTag.LOGICAL_EQUAL) || checkTokenTag(DomainTag.LOGICAL_NOT_EQUAL))
            {
                if (checkTokenTag(DomainTag.LOGICAL_EQUAL))
                {
                    parseToken(DomainTag.LOGICAL_EQUAL);
                    return new LogicalEqual(relationalExpression, parseEqulityExpression());
                }
                else
                {
                    parseToken(DomainTag.LOGICAL_NOT_EQUAL);
                    return new LogicalNotEqual(relationalExpression, parseEqulityExpression());
                }   
            }
            return relationalExpression;
        }
        // Бинарные побитовые операции
        // BitwiseANDExpressionNoIn = EqualityExpressionNoIn | BitwiseANDExpressionNoIn "&" EqualityExpressionNoIn

        // BitwiseANDExpressionNoIn = EqualityExpressionNoIn BitwiseANDExpressionNoIn'
        
        // BitwiseAndExpression = EqualityExpression [ "&" BitwiseAndExpression ]
        private Expression parseBitwiseAndExpression()
        {
            Expression equalityExpression = parseEqulityExpression();
            if (checkTokenTag(DomainTag.AND))
            {
                parseToken(DomainTag.AND);
                return new BitwiseAND(equalityExpression, parseBitwiseAndExpression());
            }
            return equalityExpression;
        }

        // BitwiseXORExpressionNoIn = BitwiseANDExpressionNoIn | BitwiseXORExpressionNoIn "^" BitwiseANDExpressionNoIn
        // BitwiseXORExpressionNoIn = BitwiseANDExpressionNoIn BitwiseXORExpressionNoIn'

        // BitwiseXorExpression = BitwiseAndExpression [ "^" BitwiseXorExpression ]
        private Expression parseBitwiseXorExpression()
        {
            Expression bitwiseAndExpression = parseBitwiseAndExpression();
            if (checkTokenTag(DomainTag.XOR))
            {
                parseToken(DomainTag.XOR);
                return new BitwiseXOR(bitwiseAndExpression, parseBitwiseXorExpression());
            }
            return bitwiseAndExpression;
        }
        // BitwiseORExpressionNoIn = BitwiseXORExpressionNoIn | BitwiseORExpressionNoIn "|" BitwiseXORExpressionNoIn
        // BitwiseORExpressionNoIn = BitwiseXORExpressionNoIn  BitwiseORExpressionNoIn'
        // BitwiseOrExpression = BitwiseXorExpression [ "|"  BitwiseOrExpression ]
        private Expression parseBitwiseOrExpression()
        {
            Expression bitwiseXorExpression = parseBitwiseXorExpression();
            if (checkTokenTag(DomainTag.OR))
            {
                parseToken(DomainTag.OR);
                return new BitwiseOR(bitwiseXorExpression, parseBitwiseOrExpression());
            }
            return bitwiseXorExpression;
        }
        // Бинарные логические операторы
        // LogicalANDExpressionNoIn = BitwiseORExpressionNoIn | LogicalANDExpressionNoIn "&&" BitwiseORExpressionNoIn 
        // LogicalANDExpressionNoIn = BitwiseORExpressionNoIn LogicalANDExpressionNoIn'
        // LogicalAndExpression = BitwiseOrExpression [ "&&" LogicalAndExpression ]
        private Expression parseLogicalAndExpression()
        {
            Expression bitwiseOrExpression = parseBitwiseOrExpression();
            if (checkTokenTag(DomainTag.LOGICAL_AND))
            {
                parseToken(DomainTag.LOGICAL_AND);
                return new LogicalAND(bitwiseOrExpression, parseLogicalAndExpression());
            }
            return bitwiseOrExpression;
        }
        // LogicalORExpressionNoIn = LogicalANDExpressionNoIn | LogicalORExpressionNoIn "||" LogicalANDExpressionNoIn
        // LogicalORExpressionNoIn = LogicalANDExpressionNoIn LogicalORExpressionNoIn'
        // LogicalOrExpression = LogicalAndExpression [ "||" LogicalOrExpression ]
        private Expression parseLogicalOrExpression()
        {
            Expression logicalAndExpression = parseLogicalAndExpression();
            if (checkTokenTag(DomainTag.LOGICAL_OR))
            {
                parseToken(DomainTag.LOGICAL_OR);
                return new LogicalOR(logicalAndExpression, parseLogicalOrExpression());
            }
            return logicalAndExpression;
        }
        // Условные оператор ( ? : )
        // ConditionalExpressionNoIn = LogicalORExpressionNoIn 
        //                              | LogicalORExpressionNoIn "?" AssignmentExpression ":" AssignmentExpressionNoIn
        // ConditionalExpressionNoIn = LogicalORExpressionNoIn [ "?" AssignmentExpression ":" AssignmentExpressionNoIn ]
        // ConditionalExpression = LogicalOrExpression [ "?" AssignmentExpression ":" AssignmentExpression ]
        private Expression parseConditionalExpression()
        {
            Expression logicalOrExpression = parseLogicalOrExpression();
            if (checkTokenTag(DomainTag.QUESTION))
            {
                parseToken(DomainTag.QUESTION);
                Expression doIfTrue = parseAssignmentExpression();
                parseToken(DomainTag.COLON);
                Expression doIfFalse = parseAssignmentExpression();
                return new Condition(logicalOrExpression, doIfTrue, doIfFalse);
            }
            return logicalOrExpression;
        }
        // TODO
        // AssignmentExpressionNoIn = ConditionalExpressionNoIn | LeftHandSideExpression AssignmentOperator AssignmentExpressionNoIn
        // AssignmentExpression = ConditionalExpression [ "=" AssignmentExpression ] 
        private Expression parseAssignmentExpression()
        {
            Expression conditionalExpression = parseConditionalExpression();
            if (checkTokenTag(DomainTag.EQUAL))
            {
                parseToken(DomainTag.EQUAL);
                return new Assignment("=", conditionalExpression, parseAssignmentExpression());
            }
            return conditionalExpression;
        }
        // Оператор запятая
        // ExpressionNoIn = AssignmentExpressionNoIn | ExpressionNoIn "," AssignmentExpressionNoIn
        // ExpressionNoIn = AssignmentExpressionNoIn ExpressionNoIn'
        // Expression = AssignmentExpression [ "," Expression ]
        private List<Expression> parseExpression()
        {
            List<Expression> expression = new List<Expression>();
            expression.Add(parseAssignmentExpression());
            if (checkTokenTag(DomainTag.COMMA))
            {
                parseToken(DomainTag.COMMA);
                expression.AddRange(parseExpression());
            }
            return expression;
        }
    }
}
