using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace JavaScriptInterpreter
{
    partial class Parser
    {
        private Queue<Token> lexems;
        private Token sym;
        private Interpreter interpreter;
        public Parser(Queue<Token> lexems, Interpreter interpreter)
        {
            this.lexems = lexems;
            this.interpreter = interpreter;
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
                interpreter.ShowErrorAndStop(sym.Coords.Starting, String.Format("Expected \"{0}\"", tag.ToString()));
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
                interpreter.ShowErrorAndStop(sym.Coords.Starting, String.Format("Expected \"{0}\"", reservedWord));
            }
            return node;
        }
        public void Start()
        {
            Node parseTree;
            parseTree = parseProgram();
            string filePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "parseTree.txt");
            parseTree.print(filePath);
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
        // Element = Statement | FunctionDeclaration
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
        // --------------------------------------Инструкции ---------------------------------------//
        // Statement = Block | VariableStatement | EmptyStatement | ExpressionStatement | IfStatement | IterationStatement | ContinueStatement | BreakStatement | ReturnStatement |
        //                   | WithStatement | LabelledStatement | SwitchStatement | ThrowStatement | TryStatement | DebuggerStatement 
        // Statement = Block | VariableStatement | EmptyStatement | ExpressionStatement | IfStatement | IterationStatement | BreakStatement | ReturnStatement |
        //                   | SwitchStatement 
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
        // Block = "{" [Statements] "}"
        private Node parseBlock()
        {
            Node block = new Node("Block");
            block.AddChild(parseToken(DomainTag.LBRACE));
            if (inFirstOfStatement())
            {
                block.AddChildren(parseStatements());
            }
            block.AddChild(parseToken(DomainTag.RBRACE));
            return block;
        }
        // Statements = Statement | Statements Statement
        // UPDATE: // Statements = Statement { Statement }
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
            initialiser.AddChild(parseAssignmentExpressionNoIn());
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
                expressionStatement.AddChild(parseExpressionNoIn());
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
            ifStatement.AddChild(parseExpressionNoIn());
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
        private Node parseIterationStatement()
        {
            Node iterationStatement = new Node("Iteration statement");
            if (checkReservedWord("do"))
            {
                iterationStatement.AddChild(parseReservedWord("do"));
                iterationStatement.AddChild(parseStatement());
                iterationStatement.AddChild(parseReservedWord("while"));
                iterationStatement.AddChild(parseToken(DomainTag.LBRACE));
                iterationStatement.AddChild(parseExpressionNoIn());
                iterationStatement.AddChild(parseToken(DomainTag.RBRACE));
                iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            }
            else if (checkReservedWord("while"))
            {
                iterationStatement.AddChild(parseReservedWord("while"));
                iterationStatement.AddChild(parseToken(DomainTag.LPARENT));
                iterationStatement.AddChild(parseExpressionNoIn());
                iterationStatement.AddChild(parseToken(DomainTag.RPARENT));
                iterationStatement.AddChild(parseStatement());
            }
            else if (checkReservedWord("for"))
            {
                iterationStatement.AddChild(parseReservedWord("for"));
                iterationStatement.AddChild(parseToken(DomainTag.LPARENT));
                if (!checkReservedWord("var"))
                {
                    if (inFirstOfExpressionNoIn())  // sym in first(expression)
                    {
                        iterationStatement.AddChild(parseExpressionNoIn());
                    }
                    iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
                    if (inFirstOfExpressionNoIn())  // sym in first(expression)
                    {
                        iterationStatement.AddChild(parseExpressionNoIn());
                    }
                    iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
                    if (inFirstOfExpressionNoIn())  // sym in first(expression)
                    {
                        iterationStatement.AddChild(parseExpressionNoIn());
                    }
                }
                else
                {
                    //  | for "(" var VariableDeclarations ";" [Expression] ";" [Expression] ) Statement 
                    iterationStatement.AddChild(parseVariableStatement());
                    if (inFirstOfExpressionNoIn())  // sym in first(expression)
                    {
                        iterationStatement.AddChild(parseExpressionNoIn());
                    }
                    iterationStatement.AddChild(parseToken(DomainTag.SEMICOLON));
                    if (inFirstOfExpressionNoIn())  // sym in first(expression)
                    {
                        iterationStatement.AddChild(parseExpressionNoIn());
                    }
                }
                iterationStatement.AddChild(parseToken(DomainTag.RPARENT));
                iterationStatement.AddChild(parseStatement());
            }
            return iterationStatement;
        }

        // Инструкция continue 
        // ContinueStatement = continue ";" | continue ( без перевода строки ) Identifier ; 
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
        private Node parseReturnStatement()
        {
            Node returnStatement = new Node("Return statement");
            returnStatement.AddChild(parseReservedWord("return"));
            returnStatement.AddChild(parseToken(DomainTag.SEMICOLON));
            // TODO/QUESTION
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
            switchStatement.AddChild(parseExpressionNoIn());
            switchStatement.AddChild(parseToken(DomainTag.RPARENT));
            switchStatement.AddChild(parseCaseBlock());
            return switchStatement;
        }
        // CaseBlock = "{" [CaseClauses] "}" | "{" [CaseClauses] DefaultClause [CaseClauses] "}"
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
        // FormalParameters = Identifier | FormalParameters , Identifier
        // QUESTION? FormalParameters = Identifier { , Identifier }
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

        // --------------------------------------Выражения ---------------------------------------//
        // Первичные выражения
        // PrimaryExpression = this | Identifier | Literal | ArrayLiteral | ObjectLiteral | "(" Expression ")"
        // PrimaryExpression = this | Identifier | Literal | ObjectLiteral | "(" ExpressionNoIn ")"
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
                primaryExpression.AddChild(parseExpressionNoIn());
                primaryExpression.AddChild(parseToken(DomainTag.RPARENT));
            }
            return primaryExpression;
        }
        // Literal = NullLiteral | BooleanLiteral | NumericLiteral | StringLiteral 
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
                propertyAssignment.AddChild(parseAssignmentExpressionNoIn());
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
        private Node parseMemberExpression()
        {
            Node memberExpression = new Node("Member expression");
            if (inFirstOfPrimaryExpression())   // Sym in first(PrimaryExpression)
            {
                memberExpression.AddChild(parsePrimaryExpression());
            }
            else if (checkReservedWord("function"))
            {
                memberExpression.AddChild(parseFunctionExpression());
            }
            else if (inFirstOfMemberExpression()) // Sym in first(MemberExpression) 
            {
                memberExpression.AddChild(parseMemberExpression());
                if (checkTokenTag(DomainTag.LSBRACKET))
                {
                    memberExpression.AddChild(parseToken(DomainTag.LSBRACKET));
                    memberExpression.AddChild(parseExpressionNoIn());
                    memberExpression.AddChild(parseToken(DomainTag.RSBRACKET));
                }
                else //if (checkTokenTag(DomainTag.POINT))
                {
                    memberExpression.AddChild(parseToken(DomainTag.POINT));
                    memberExpression.AddChild(parseToken(DomainTag.IDENT));
                }
            }
            else //if (checkReservedWord("new"))
            {
                memberExpression.AddChild(parseMemberExpression());
                memberExpression.AddChild(parseArguments()); // TODO: addchildrens?
            }
            return memberExpression;
        }

        // NewExpression = MemberExpression | new NewExpression
        private Node parseNewExpression()
        {
            Node newExpression = new Node("New expression");
            if (inFirstOfMemberExpression()) // sym in first(MemberExpression)
            {
                newExpression.AddChild(parseMemberExpression());
            }
            else
            {
                newExpression.AddChild(parseReservedWord("new"));
                newExpression.AddChild(parseNewExpression());
            }
            return newExpression;
        }
        // CallExpression = MemberExpression Arguments | CallExpression Arguments | CallExpression "[" Expression "]" 
        //                  | CallExpression . Identifier

        private Node parseCallExpression()
        {
            Node callExpression = new Node("Call expression");
            if (inFirstOfMemberExpression()) // sym in first(memberexpression)
            {
                callExpression.AddChild(parseMemberExpression());
                callExpression.AddChild(parseArguments());
            }
            else if (inFirstOfCallExpression()) // sym in first(callexpression)
            {
                callExpression.AddChild(parseCallExpression());
                if (checkTokenTag(DomainTag.LPARENT))
                {
                    callExpression.AddChild(parseArguments());
                }
                else if (checkTokenTag(DomainTag.LBRACE))
                {
                    callExpression.AddChild(parseToken(DomainTag.LBRACE));
                    callExpression.AddChild(parseExpressionNoIn());
                    callExpression.AddChild(parseToken(DomainTag.RBRACE));
                }
                else
                {
                    callExpression.AddChild(parseToken(DomainTag.POINT));
                    callExpression.AddChild(parseToken(DomainTag.IDENT));
                }
            }
            return callExpression;
        }
        // Arguments = "(" [ArgumentList] ")"
        private Node parseArguments()
        {
            Node arguments = new Node("Arguments");
            arguments.AddChild(parseToken(DomainTag.LPARENT));
            if (checkTokenTag(DomainTag.IDENT))
            {
                arguments.AddChildren(parseArgumentList());
            }
            arguments.AddChild(parseToken(DomainTag.RPARENT));
            return arguments;
        }
        // ArgumentList = [ArugmentList ","] AssignmentExpression
        private List<Node> parseArgumentList()
        {
            List<Node> argumentList = new List<Node>();
            if (inFirstOfAssignmentExpressionNoIn()) // sym in first(assignmentexpression)
            {
                argumentList.AddRange(parseArgumentList());
                argumentList.Add(parseToken(DomainTag.COMMA));
            }
            argumentList.Add(parseAssignmentExpressionNoIn());
            return argumentList;
        }
        // LeftHandSideExpression = NewExpression | CallExpression
        private Node parseLeftHandSideExpression()
        {
            Node leftHandSideExpression = new Node("Left-hand-side expression");
            if (inFirstOfNewExpression()) // sym in NewExpression
            {
                leftHandSideExpression.AddChild(parseNewExpression());
            }
            else
            {
                leftHandSideExpression.AddChild(parseCallExpression());
            }
            return leftHandSideExpression;
        }

        // Потсфиксные выражения
        // PostfixExpression = LeftHandSideExpression | LeftHandSideExpression "++" | LeftHandSideExpression "--"
        private Node parsePostfixExpression()
        {
            Node postfixExpression = new Node("Postfix expression");
            postfixExpression.AddChild(parseLeftHandSideExpression());
            if (checkTokenTag(DomainTag.INCREMENT))
            {
                postfixExpression.AddChild(parseToken(DomainTag.INCREMENT));
            }
            else if (checkTokenTag(DomainTag.DECREMENT))
            {
                postfixExpression.AddChild(parseToken(DomainTag.DECREMENT));
            }
            return postfixExpression;
        }


        // Унарные операторы
        // UnaryExpression = PostfixExpression | delete UnaryExpression | void UnaryExpression | typeof UnarryExpression |
        //                      | "++" UnaryExpression | "--" UnaryExpression | "+" UnaryExpression | "-" UnaryExpression
        //                      | "~" UnaryExpresssion | "!" UnaryExpression 
        // UnaryExpression = PostfixExpression | delete UnaryExpression | void UnaryExpression | typeof UnarryExpression |
        //                      | "++" UnaryExpression | "--" UnaryExpression | "+" UnaryExpression | "-" UnaryExpression

        private Node parseUnaryExpression()
        {
            Node unaryExpression = new Node("Unary expression");
            if (inFirstOfPostfixExpression())   // sym in first(postfixexpression)
            {
                unaryExpression.AddChild(parsePostfixExpression());
            }
            else if (checkReservedWord("delete"))
            {
                unaryExpression.AddChild(parseReservedWord("delete"));
                unaryExpression.AddChild(parseUnaryExpression());
            }
            else if (checkReservedWord("void"))
            {
                unaryExpression.AddChild(parseReservedWord("void"));
                unaryExpression.AddChild(parseUnaryExpression());
            }
            else if (checkReservedWord("typeof"))
            {
                unaryExpression.AddChild(parseReservedWord("typeof"));
                unaryExpression.AddChild(parseUnaryExpression());
            }
            else if (checkTokenTag(DomainTag.INCREMENT))
            {
                unaryExpression.AddChild(parseToken(DomainTag.INCREMENT));
                unaryExpression.AddChild(parseUnaryExpression());
            }
            else if (checkTokenTag(DomainTag.DECREMENT))
            {
                unaryExpression.AddChild(parseToken(DomainTag.DECREMENT));
                unaryExpression.AddChild(parseUnaryExpression());
            }
            else if (checkTokenTag(DomainTag.PLUS))
            {
                unaryExpression.AddChild(parseToken(DomainTag.PLUS));
                unaryExpression.AddChild(parseUnaryExpression());
            }
            else if (checkTokenTag(DomainTag.MINUS))
            {
                unaryExpression.AddChild(parseToken(DomainTag.MINUS));
                unaryExpression.AddChild(parseUnaryExpression());
            }
            return unaryExpression;
        }

        // Мультипликативные
        // MultiplicativeExpression = UnaryExpression | MultiplicativeExpression "*" UnaryExpression |
        //                              | MultiplicativeExpression "/" UnaryExpression | MultiplicativeExpression "%" UnaryExpression
        private Node parseMultiplicativeExpression()
        {
            Node multiplicativeExpression = new Node("Multiplicative expression");
            if (inFirstOfUnaryExpression()) // sym in first(unaryExpression)
            {
                multiplicativeExpression.AddChild(parseUnaryExpression());
            }
            else
            {
                multiplicativeExpression.AddChild(parseMultiplicativeExpression());
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
                multiplicativeExpression.AddChild(parseUnaryExpression());
            }
            return multiplicativeExpression;
        }
        // Аддитивные операторы
        // AdditiveExpression = MultiplicativeExpression | AdditiveExpression "+" MultiplicativeExpression |
        //                          | AdditiveExpression "-" MultiplicativeExpression
        private Node parseAdditiveExpression()
        {
            Node additiveExpression = new Node("Additive expression");
            if (inFirstOfMultiplicativeExpression()) // sym in first MultiplicativeExpression
            {
                additiveExpression.AddChild(parseMultiplicativeExpression());
            }
            else
            {
                additiveExpression.AddChild(parseAdditiveExpression());
                if (checkTokenTag(DomainTag.PLUS))
                {
                    additiveExpression.AddChild(parseToken(DomainTag.PLUS));
                }
                else
                {
                    additiveExpression.AddChild(parseToken(DomainTag.MINUS));
                }
                additiveExpression.AddChild(parseMultiplicativeExpression());
            }
            return additiveExpression;
        }
        // Операторы побитового сдвига
        // ShiftExpression = AdditiveExpression | ShiftExpression "<<" AdditiveExrpession 
        //                      | ShiftExpression ">>" AdditiveExpression
        private Node parseShiftExpression()
        {
            Node shiftExpression = new Node("Shift expression");
            if (inFirstOfAdditiveExpression()) // sym in first(additiveExpression)
            {
                shiftExpression.AddChild(parseAdditiveExpression());
            }
            else
            {
                shiftExpression.AddChild(parseShiftExpression());
                if (checkTokenTag(DomainTag.LSHIFT))
                {
                    shiftExpression.AddChild(parseToken(DomainTag.LSHIFT));
                }
                else
                {
                    shiftExpression.AddChild(parseToken(DomainTag.RSHIFT));
                }
                shiftExpression.AddChild(parseAdditiveExpression());
            }
            return shiftExpression;
        }
        // Операторы отношения
        // RelationalExpressionNoIn = ShiftExpression | RelationalExpressionNoIn "<" ShiftExpression 
        //                              | RelationalExpressionNoIn ">" ShiftExpression 
        //                              | RelationalExpressionNoIn "<=" ShiftExpression 
        //                              | RelationalExpressionNoIn ">=" ShiftExpression
        //                              | RelationalExpressionNoIn instanceof ShiftExpression (not implemented)
        private Node parseRelationExpressionNoIn()
        {
            Node relationExpressionNoIn = new Node("Relation expression (no in)");
            if (inFirstOfShiftExpression())   // symbol in first (ShiftExpression)
            {
                relationExpressionNoIn.AddChild(parseShiftExpression());
            }
            else
            {
                relationExpressionNoIn.AddChild(parseRelationExpressionNoIn());
                if (checkTokenTag(DomainTag.LESS))
                {
                    relationExpressionNoIn.AddChild(parseToken(DomainTag.LESS));
                }
                else if (checkTokenTag(DomainTag.LARGER))
                {
                    relationExpressionNoIn.AddChild(parseToken(DomainTag.LARGER));
                }
                else if (checkTokenTag(DomainTag.LESS_OR_EQUAL))
                {
                    relationExpressionNoIn.AddChild(parseToken(DomainTag.LESS_OR_EQUAL));
                }
                else// if (checkTokenTag(DomainTag.LARGER_OR_EQUAL))
                {
                    relationExpressionNoIn.AddChild(parseToken(DomainTag.LARGER_OR_EQUAL));
                }
                relationExpressionNoIn.AddChild(parseShiftExpression());
            }
            return relationExpressionNoIn;
        }
        // Операторы равенства
        //  EqualityExpressionNoIn = RelationalExpressionNoIn | EqualityExpressionNoIn "==" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "!=" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "===" RelationalExpressionNoIn (not implemented)
        //                              | EqualityExpressionNoIn "!==" RelationalExpressionNoIn (not implemented)
        private Node parseEqulityExpressionNoIn()
        {
            Node equlityExpressionNoIn = new Node("Equlity expression (no in)");
            if (inFirstOfRelationalExpressionNoIn()) // sym in first(relationalExpressionNoIn)
            {
                equlityExpressionNoIn.AddChild(parseRelationExpressionNoIn());
            }
            else
            {
                equlityExpressionNoIn.AddChild(parseEqulityExpressionNoIn());
                if (checkTokenTag(DomainTag.LOGICAL_EQUAL))
                {
                    equlityExpressionNoIn.AddChild(parseToken(DomainTag.LOGICAL_EQUAL));
                }
                else
                {
                    equlityExpressionNoIn.AddChild(parseToken(DomainTag.LOGICAL_NOT_EQUAL));
                }
                equlityExpressionNoIn.AddChild(parseRelationExpressionNoIn());
            }
            return equlityExpressionNoIn;
        }
        // Бинарные побитовые операции
        // BitwiseANDExpressionNoIn = EqualityExpressionNoIn | BitwiseANDExpressionNoIn "&" EqualityExpressionNoIn
        private Node parseBitwiseANDExpressionNoIn()
        {
            Node bitwiseANDExpressionNoIn = new Node("Bitwise AND expression (no in)");
            if (inFirstOfEqualityExpressionNoIn()) // sym in first EqualiteExpressionNoIN
            {
                bitwiseANDExpressionNoIn.AddChild(parseEqulityExpressionNoIn());
            }
            else
            {
                bitwiseANDExpressionNoIn.AddChild(parseBitwiseANDExpressionNoIn());
                bitwiseANDExpressionNoIn.AddChild(parseToken(DomainTag.LOGICAL_AND));
                bitwiseANDExpressionNoIn.AddChild(parseEqulityExpressionNoIn());
            }
            return bitwiseANDExpressionNoIn;
        }
        // BitwiseXORExpressionNoIn = BitwiseANDExpressionNoIn | BitwiseXORExpressionNoIn "^" BitwiseANDExpressionNoIn
        private Node parseBitwiseXORExpressionNoIn()
        {
            Node bitwiseXORExpressionNoIn = new Node("Bitwise XOR expressionNoIn");
            if (inFirstOfEqualityExpressionNoIn())
            {
                bitwiseXORExpressionNoIn.AddChild(parseBitwiseANDExpressionNoIn());
            }
            else
            {
                bitwiseXORExpressionNoIn.AddChild(parseBitwiseXORExpressionNoIn());
                bitwiseXORExpressionNoIn.AddChild(parseToken(DomainTag.XOR));
                bitwiseXORExpressionNoIn.AddChild(parseBitwiseANDExpressionNoIn());
            }
            return bitwiseXORExpressionNoIn;
        }

        // BitwiseORExpressionNoIn = BitwiseXORExpressionNoIn | BitwiseORExpressionNoIn "|" BitwiseXORExpressionNoIn
        private Node parseBitwiseORExpressionNoIn()
        {
            Node bitwiseORExpressionNoIn = new Node("Bitwise OR expression (no in)");

            if (inFirstOfBitwiseXORExpressionNoIn()) // sym in first BitwiseXORExpressionNoIn
            {
                bitwiseORExpressionNoIn.AddChild(parseBitwiseXORExpressionNoIn());
            }
            else
            {
                bitwiseORExpressionNoIn.AddChild(parseBitwiseORExpressionNoIn());
                bitwiseORExpressionNoIn.AddChild(parseToken(DomainTag.OR));
                bitwiseORExpressionNoIn.AddChild(parseBitwiseXORExpressionNoIn());
            }
            return bitwiseORExpressionNoIn;
        }
        // Бинарные логические операторы
        // LogicalANDExpressionNoIn = BitwiseORExpressionNoIn | LogicalANDExpressionNoIn "&&" BitwiseORExpressionNoIn 
        private Node parseLogicalANDExpressionNoIn()
        {
            Node logicalANDExpressionNoIn = new Node("Logical AND expression (no in)");
            if (inFirstOfBitwiseORExpressionNoIn()) // sym in first[bitwiseorexpressionnoin]
            {
                logicalANDExpressionNoIn.AddChild(parseBitwiseORExpressionNoIn());
            }
            else
            {
                logicalANDExpressionNoIn.AddChild(parseLogicalANDExpressionNoIn());
                logicalANDExpressionNoIn.AddChild(parseToken(DomainTag.LOGICAL_AND));
                logicalANDExpressionNoIn.AddChild(parseBitwiseORExpressionNoIn());
            }
            return logicalANDExpressionNoIn;
        }
        // LogicalORExpressionNoIn = LogicalANDExpressionNoIn | LogicalORExpressionNoIn "||" LogicalANDExpressionNoIn
        private Node parseLogicalORExpressionNoIn()
        {
            Node logicalORExpressionNoIn = new Node("Logical OR expression (no in)");
            if (inFirstOfLogicalANDExpressionNoIn()) // sym in first[LogicalANDExpressionNoIn]
            {
                logicalORExpressionNoIn.AddChild(parseLogicalANDExpressionNoIn());
            }
            else
            {
                logicalORExpressionNoIn.AddChild(parseLogicalORExpressionNoIn());
                logicalORExpressionNoIn.AddChild(parseToken(DomainTag.LOGICAL_OR));
                logicalORExpressionNoIn.AddChild(parseLogicalANDExpressionNoIn());
            }
            return logicalORExpressionNoIn;
        }
        // Условные оператор ( ? : )
        // ConditionalExpressionNoIn = LogicalORExpressionNoIn 
        //                              | LogicalORExpressionNoIn "?" AssignmentExpression ":" AssignmentExpressionNoIn
        private Node parseConditionalExpressionNoIn()
        {
            Node conditionalExpressionNoIn = new Node("Conditional expression (no in)");
            conditionalExpressionNoIn.AddChild(parseLogicalORExpressionNoIn());
            if (checkTokenTag(DomainTag.QUESTION))
            {
                conditionalExpressionNoIn.AddChild(parseToken(DomainTag.QUESTION));
                conditionalExpressionNoIn.AddChild(parseAssignmentExpressionNoIn());
                conditionalExpressionNoIn.AddChild(parseToken(DomainTag.COLON));
                conditionalExpressionNoIn.AddChild(parseAssignmentExpressionNoIn());
            }
            return conditionalExpressionNoIn;
        }
        // AssignmentExpressionNoIn = ConditionalExpressionNoIn | LeftHandSideExpression AssignmentOperator AssignmentExpressionNoIn
        private Node parseAssignmentExpressionNoIn()
        {
            Node assignmentExpressionNoIn = new Node("Assignment expression (no in)");
            if (inFirstOfConditionalExpressionNoIn()) // first in [condit exprs]
            {
                assignmentExpressionNoIn.AddChild(parseConditionalExpressionNoIn());
            }
            else
            {
                assignmentExpressionNoIn.AddChild(parseLeftHandSideExpression());
                // not implement +=, -=, /=, %=...
                assignmentExpressionNoIn.AddChild(parseToken(DomainTag.EQUAL));
                assignmentExpressionNoIn.AddChild(parseAssignmentExpressionNoIn());
            }
            return assignmentExpressionNoIn;
        }
        // Оператор запятая
        // ExpressionNoIn = AssignmentExpressionNoIn | ExpressionNoIn "," AssignmentExpressionNoIn
        private Node parseExpressionNoIn()
        {
            Node expressionNoIn = new Node("Expression (no in)");
            if (inFirstOfAssignmentExpressionNoIn()) // sym in first assginmentNoIn
            {
                expressionNoIn.AddChild(parseAssignmentExpressionNoIn());
            }
            else
            {
                expressionNoIn.AddChild(parseExpressionNoIn());
                expressionNoIn.AddChild(parseToken(DomainTag.COMMA));
                expressionNoIn.AddChild(parseAssignmentExpressionNoIn());
            }
            return expressionNoIn;
        }

        
    }
}
