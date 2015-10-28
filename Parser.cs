using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return lexems.Dequeue();
        }
        private bool checkTokenTag(DomainTag tag)
        {
            return sym.Tag == tag;
        }
        private bool checkReservedWord(string reservedWord)
        {
            return checkTokenTag(DomainTag.RESERVED_WORD) && ((ReservedWordToken)sym).ReservedWord.Equals(reservedWord);
        }
        private void parseTerminal(DomainTag tag)
        {
            if (checkTokenTag(tag))
            {
                sym = nextToken();
            }
            else
            {
                interpreter.ShowErrorAndStop(sym.Coords.Starting, String.Format("Expected \"{0}\"", tag.ToString()));
            }
        }
        private void parseReservedWord(string reservedWord)
        {
            if (checkReservedWord(reservedWord))
            {
                sym = nextToken();
            }
            else
            {
                interpreter.ShowErrorAndStop(sym.Coords.Starting, String.Format("Expected \"{0}\"", reservedWord));
            }
        }
        public void Start()
        {
            parseProgram();
        }
        // --------------------------------------Программа ---------------------------------------//
        // Program = Elements
        private void parseProgram()
        {
            parseElements();
        }
        // Elements = { Element }
        private void parseElements()
        {
            while (inFirstOfStatement() || checkReservedWord("function")) 
            {
                parseElement();
            }
        }
        // Element = Statement | FunctionDeclaration
        private void parseElement()
        {
            if (inFirstOfStatement())
            {
                parseStatement();
            }
            else 
            {
                parseFunctionDeclaration();
            }
        }
        // --------------------------------------Инструкции ---------------------------------------//
        // Statement = Block | VariableStatement | EmptyStatement | ExpressionStatement | IfStatement | IterationStatement | ContinueStatement | BreakStatement | ReturnStatement |
        //                   | WithStatement | LabelledStatement | SwitchStatement | ThrowStatement | TryStatement | DebuggerStatement 
        // Statement = Block | VariableStatement | EmptyStatement | ExpressionStatement | IfStatement | IterationStatement | BreakStatement | ReturnStatement |
        //                   | SwitchStatement 
        private void parseStatement()
        {
            if (checkTokenTag(DomainTag.LBRACE))
            {
                parseBlock();
            }
            else if (checkReservedWord("var"))
            {
                parseVariableStatement();
            }
            else if (checkTokenTag(DomainTag.SEMICOLON))
            {
                parseEmptyStatement();
            }
            else if (inFirstOfExpressionStatement()) // TODO: sym in first(ExpressionStatement)
            {
                parseExpressionStatement();
            }
            else if (checkReservedWord("if"))
            {
                parseIfStatement();
            }
            else if (checkReservedWord("for") || checkReservedWord("while") || checkReservedWord("do"))
            {
                parseIterationStatement();
            }
            else if (checkReservedWord("break"))
            {
                parseBreakStatement();
            }
            else if (checkReservedWord("continue"))
            {
                parseContinueStatement();
            }
            else if (checkReservedWord("return"))
            {
                parseReturnStatement();
            }
            else if (checkReservedWord("switch"))
            {
                parseSwitchStatement();
            }
            
        }
        // Блоки
        // Block = "{" [Statements] "}"
        private void parseBlock()
        {
            parseTerminal(DomainTag.LBRACE);
            if (inFirstOfStatement())
            {
                parseStatements();
            }
            parseTerminal(DomainTag.RBRACE);
        }
        // Statements = Statement | Statements Statement
        private void parseStatements()
        {
            while (inFirstOfStatement()) 
            {
                parseStatement();
            }
        }
        // Объявление переменной
        // VariableStatement = var VariableDeclarations ";"
        private void parseVariableStatement()
        {
            parseReservedWord("var");
            parseVariableDeclarations();
            parseTerminal(DomainTag.SEMICOLON);
        }

        // VariableDeclarations = VariableDeclaration { "," VariableDeclaration }
        private void parseVariableDeclarations()
        {
            do parseVariableDeclartion();
            while (checkTokenTag(DomainTag.COMMA));
        }
        // VariableDeclaration = Identifier [ Initialiser ]
        private void parseVariableDeclartion()
        {
            parseTerminal(DomainTag.IDENT);
            // TODO
            if (true)   // Sym in first(Initialiser)
            {
                parseInitialiser();
            }
        }
        // Initialiser = AssignmentExpression
        private void parseInitialiser()
        {
            parseAssignmentExpressionNoIn();
        }

        // Пустая строка
        // EmptyStatement = ;
        private void parseEmptyStatement()
        {
            parseTerminal(DomainTag.SEMICOLON);
        }

        // Инструкция выражение
        // ExpressionStatement = (не начинается с {, function ) Expression ;
        private void parseExpressionStatement()
        {
            if (!checkTokenTag(DomainTag.LBRACE) && !checkReservedWord("function"))
            {
                parseExpressionNoIn();
            }
            parseTerminal(DomainTag.SEMICOLON);
        }

        // Инструкция if
        // IfStatement = if "(" Expression ")" Statement [ else Statement ] 
        private void parseIfStatement()
        {
            parseReservedWord("if");
            parseTerminal(DomainTag.LPARENT);
            parseExpressionNoIn();
            parseTerminal(DomainTag.RPARENT);
            parseStatement();
            if (checkReservedWord("else"))
            {
                parseReservedWord("else");
                parseStatement();
            }
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
        private void parseIterationStatement()
        {
            if (checkReservedWord("do"))
            {
                parseReservedWord("do");
                parseStatement();
                parseReservedWord("while");
                parseTerminal(DomainTag.LBRACE);
                parseExpressionNoIn();
                parseTerminal(DomainTag.RBRACE);
                parseTerminal(DomainTag.SEMICOLON);
            }
            else if (checkReservedWord("while"))
            {
                parseReservedWord("while");
                parseTerminal(DomainTag.LPARENT);
                parseExpressionNoIn();
                parseTerminal(DomainTag.RPARENT);
                parseStatement();
            }
            else if (checkReservedWord("for"))
            {
                parseReservedWord("for");
                parseTerminal(DomainTag.LPARENT);
                // TODO
                if (!checkReservedWord("var"))
                {
                    if (true)  // sym in first(expression)
                    {
                        parseExpressionNoIn();
                    }
                    parseTerminal(DomainTag.SEMICOLON);
                    if (true)  // sym in first(expression)
                    {
                        parseExpressionNoIn();
                    }
                    parseTerminal(DomainTag.SEMICOLON);
                    if (true)  // sym in first(expression)
                    {
                        parseExpressionNoIn();
                    }
                }
                else
                {
                    //  | for "(" var VariableDeclarations ";" [Expression] ";" [Expression] ) Statement 
                    parseVariableStatement();
                    if (true)  // sym in first(expression)
                    {
                        parseExpressionNoIn();
                    }
                    parseTerminal(DomainTag.SEMICOLON);
                    if (true)  // sym in first(expression)
                    {
                        parseExpressionNoIn();
                    }
                }
                parseTerminal(DomainTag.RPARENT);
                parseStatement();
            }
        }

        // Инструкция continue 
        // ContinueStatement = continue ";" | continue ( без перевода строки ) Identifier ; 
        private void parseContinueStatement()
        {
            parseReservedWord("continue");
            parseTerminal(DomainTag.SEMICOLON);
            // TODO/QUESTION
            // continue ( без перевода строки ) Identifier ; 
        }
        // Инструкция break
        // BreakStatement = break ";" | break ( без перевода строки ) Identifier ;
        private void parseBreakStatement()
        {
            parseReservedWord("break");
            parseTerminal(DomainTag.SEMICOLON);
            // TODO/QUESTION
            // break ( без перевода строки ) Identifier ;
        }

        // Инструкция return 
        // ReturnStatement = return ";" | return ( без перевода строки ) Expression;
        private void parseReturnStatement()
        {
            parseReservedWord("return");
            parseTerminal(DomainTag.SEMICOLON);
            // TODO/QUESTION
            // return ( без перевода строки ) Expression;
        }
        
        // Инструкция switch
        // SwitchStatement = switch "(" Expression ")" CaseBlock
        private void parseSwitchStatement()
        {
            parseReservedWord("switch");
            parseTerminal(DomainTag.LPARENT);
            parseExpressionNoIn();
            parseTerminal(DomainTag.RPARENT);
            parseCaseBlock();
        }
        // CaseBlock = "{" [CaseClauses] "}" | "{" [CaseClauses] DefaultClause [CaseClauses] "}"
        private void parseCaseBlock()
        {
            parseTerminal(DomainTag.LBRACE);
            if (checkReservedWord("case"))
            {
                parseCaseCaluses();
            }
            if (checkReservedWord("default"))
            {
                parseDefaultClause();
                if (checkReservedWord("case")) 
                parseCaseCaluses();
            }
            parseTerminal(DomainTag.RBRACE);
        }
        // CaseClauses = CaseClause | CaseClauses CaseClause
        // QUESTION CaseClauses = CaseClause { CaseClause }
        private void parseCaseCaluses()
        {
            do parseCaseClause();
            while (checkReservedWord("case"));
        }
        // CaseClause = case Expression ":" Statements
        private void parseCaseClause()
        {
            parseReservedWord("case");
            parseTerminal(DomainTag.COLON);
            parseStatements();
        }
        // DefaultClause = default ":" [Statements]
        private void parseDefaultClause()
        {
            parseReservedWord("default");
            parseTerminal(DomainTag.COLON);
            if (inFirstOfStatement()) 
            {
                parseStatements();
            }
        }

        // Объявление функции
        // FunctionDeclaration = function Identifier "(" [FormalParameters] ")" "{" FunctionBody "}"
        private void parseFunctionDeclaration()
        {
            parseReservedWord("function");
            parseTerminal(DomainTag.IDENT);
            parseTerminal(DomainTag.LPARENT);
            if (checkTokenTag(DomainTag.IDENT)) 
            {
                parseFormalParameters();
            }
            parseTerminal(DomainTag.RPARENT);
            parseTerminal(DomainTag.LBRACE);
            parseFunctionBody();
            parseTerminal(DomainTag.RBRACE);
        }

        // FunctionExpression = function [Identifier] "(" [FormalParameters] ")" "{" FunctionBody "}"
        private void parseFunctionExpression()
        {
            parseReservedWord("function");
            if (checkTokenTag(DomainTag.IDENT))
            {
                parseTerminal(DomainTag.IDENT);
            }
            parseTerminal(DomainTag.LPARENT);
            if (checkTokenTag(DomainTag.IDENT)) 
            {
                parseFormalParameters();
            }
            parseTerminal(DomainTag.RPARENT);
            parseTerminal(DomainTag.LBRACE);
            parseFunctionBody();
            parseTerminal(DomainTag.RBRACE);
        }
        // FormalParameters = Identifier | FormalParameters , Identifier
        // QUESTION? FormalParameters = Identifier { , Identifier }
        private void parseFormalParameters()
        {
            parseTerminal(DomainTag.IDENT);
            while (checkTokenTag(DomainTag.COMMA))
            {
                parseTerminal(DomainTag.COMMA);
                parseTerminal(DomainTag.IDENT);
            }
        }
        // FunctionBody = Elements
        private void parseFunctionBody()
        {
            parseElements();
        }


        // --------------------------------------Выражения ---------------------------------------//
        // Первичные выражения
        // PrimaryExpression = this | Identifier | Literal | ArrayLiteral | ObjectLiteral | "(" Expression ")"
        // PrimaryExpression = this | Identifier | Literal | ObjectLiteral | "(" ExpressionNoIn ")"
        private void parsePrimaryExpression()
        {
            if (checkReservedWord("this"))
            {
                parseReservedWord("this");
            }
            else if (checkTokenTag(DomainTag.IDENT))
            {
                parseTerminal(DomainTag.IDENT);
            }
            else if (inFirstOfLiteral())
            {
                parseLiteral();
            }
            else if (inFirstOfObjectLiteral())
            {
                parseObjectLiteral();
            }
            else// if (checkTokenTag(DomainTag.LPARENT)){
            {
                parseTerminal(DomainTag.LPARENT);
                parseExpressionNoIn();
                parseTerminal(DomainTag.RPARENT);
            }
        }
        //Literal = NullLiteral | BooleanLiteral | NumericLiteral | StringLiteral 
        private void parseLiteral()
        {
            if (checkReservedWord("null"))
            {
                parseReservedWord("null");
            }
            else if (checkReservedWord("true"))
            {
                parseReservedWord("true");
            }
            else if (checkReservedWord("false"))
            {
                parseReservedWord("false");
            }
            else if (checkTokenTag(DomainTag.NUMBER))
            {
                parseTerminal(DomainTag.NUMBER);
            }
            else
            {
                parseTerminal(DomainTag.STRING);
            }
        }

        // Инциализация объекта
        // ObjectLiteral = "{" "}" | "{" PropertyNamesAndValues "}"
        private void parseObjectLiteral()
        {
            parseTerminal(DomainTag.LBRACE);
            if (true) // sym in first(propertyNamesAndValues)
            {
                parsePropertyNamesAndValues();
            }
            parseTerminal(DomainTag.RBRACE);

        }
        // PropertyNamesAndValues = { PropertyNamesAndValues "," } PropertyAssignment 
        private void parsePropertyNamesAndValues()
        {
            while (inFirstOfPropertyNamesAndValues()) // sym in first(propertyNamesAndValues)
            {
                parsePropertyNamesAndValues();
                parseTerminal(DomainTag.COMMA);
            }
            parsePropertyAssignment();
        }
        
        // PropertyAssignment = PropertyName ":" AssignmentExpression | get PropertyName "(" ")" "{" FunctionBody "}" | 
        //                          | set PropertyName "(" PropertySetParameters ")" "{" FunctionBody "}".
        private void parsePropertyAssignment()
        {
            if (inFirstOfProperyName()) // First(Propertyname)
            {
                parsePropertyName();
                parseTerminal(DomainTag.COLON);
                parseAssignmentExpressionNoIn();
            }
            else if (checkReservedWord("get"))
            {
                parseReservedWord("get");
                parsePropertyName();
                parseTerminal(DomainTag.LPARENT);
                parseTerminal(DomainTag.RPARENT);
                parseTerminal(DomainTag.LBRACE);
                parseFunctionBody();
                parseTerminal(DomainTag.RBRACE);
            }
            else
            {
                parseReservedWord("set");
                parsePropertyName();
                parseTerminal(DomainTag.LPARENT);
                parsePropertySetParameters();
                parseTerminal(DomainTag.RPARENT);
                parseTerminal(DomainTag.LBRACE);
                parseFunctionBody();
                parseTerminal(DomainTag.RBRACE);
            }
        }

        // QUESTION = NumericLiteral?
        // PropertyName = IdentifierName | StringLiteral | NumericLiteral
        // PropertyName = IdentifierName | StringLiteral
        private void parsePropertyName()
        {
            if (checkTokenTag(DomainTag.IDENT))
            {
                parseTerminal(DomainTag.IDENT);
            }
            else
            {
                parseTerminal(DomainTag.STRING);
            }
        }
        // TODO IN FUTURE
        private void parsePropertySetParameters()
        {

        }


        // Левосторонние выражения
        // MemberExpression = PrimaryExpression | FunctionExpression | MemberExpression "[" Expression "]" 
        //                      | MemberExpression "." Identifier | new MemberExpression Arguments
        // QUESTION !!!!!
        private void parseMemberExpression()
        {
            // TODO
            if (inFirstOfPrimaryExpression())   // Sym in first(PrimaryExpression)
            {
                parsePrimaryExpression();
            }
            else if (checkReservedWord("function"))
            {
                parseFunctionExpression();
            }else if (inFirstOfMemberExpression()) // Sym in first(MemberExpression) 
            {
                parseMemberExpression();
                if (checkTokenTag(DomainTag.LSBRACKET))
                {
                    parseTerminal(DomainTag.LSBRACKET);
                    parseExpressionNoIn();
                    parseTerminal(DomainTag.RSBRACKET);
                }
                else //if (checkTokenTag(DomainTag.POINT))
                {
                    parseTerminal(DomainTag.POINT);
                    parseTerminal(DomainTag.IDENT);
                }
            }
            else //if (checkReservedWord("new"))
            {
                parseMemberExpression();
                parseArguments();
            }
        }

        // NewExpression = MemberExpression | new NewExpression
        private void parseNewExpression()
        {
            // TODO
            if (inFirstOfMemberExpression()) // sym in first(MemberExpression)
            {
                parseMemberExpression();
            }
            else
            {
                parseReservedWord("new");
                parseNewExpression();
            }
        }
        // CallExpression = MemberExpression Arguments | CallExpression Arguments | CallExpression "[" Expression "]" 
        //                  | CallExpression . Identifier

        private void parseCallExpression()
        {
            if (inFirstOfMemberExpression()) // sym in first(memberexpression)
            {
                parseMemberExpression();
                parseArguments();
            }
            else if (inFirstOfCallExpression()) // sym in first(callexpression)
            {
                parseCallExpression();
                if (checkTokenTag(DomainTag.LPARENT))
                {
                    parseArguments();
                }
                else if (checkTokenTag(DomainTag.LBRACE))
                {
                    parseTerminal(DomainTag.LBRACE);
                    parseExpressionNoIn();
                    parseTerminal(DomainTag.RBRACE);
                }
                else
                {
                    parseTerminal(DomainTag.POINT);
                    parseTerminal(DomainTag.IDENT);
                }
            }
        }
        // Arguments = "(" [ArgumentList] ")"
        private void parseArguments()
        {
            parseTerminal(DomainTag.LPARENT);
            if (checkTokenTag(DomainTag.IDENT))
            {
                parseArgumentList();
            }
            parseTerminal(DomainTag.RPARENT);
        }
        // ArgumentList = [ArugmentList ","] AssignmentExpression
        private void parseArgumentList()
        {
            if (true) // sym in first(assignmentexpression)
            {
                parseArgumentList();
                parseTerminal(DomainTag.COMMA);
            }
            parseAssignmentExpressionNoIn();
        }
        // LeftHandSideExpression = NewExpression | CallExpression
        private void parseLeftHandSideExpression()
        {
            if (inFirstOfNewExpression()) // sym in NewExpression
            {
                parseNewExpression();
            }
            else
            {
                parseCallExpression();
            }
        }

        // Потсфиксные выражения
        // PostfixExpression = LeftHandSideExpression | LeftHandSideExpression "++" | LeftHandSideExpression "--"
        private void parsePostfixExpression()
        {
            parseLeftHandSideExpression();
            if (checkTokenTag(DomainTag.INCREMENT))
            {
                parseTerminal(DomainTag.INCREMENT);
            }
            else if (checkTokenTag(DomainTag.DECREMENT))
            {
                parseTerminal(DomainTag.DECREMENT);
            }
        }


        // Унарные операторы
        // UnaryExpression = PostfixExpression | delete UnaryExpression | void UnaryExpression | typeof UnarryExpression |
        //                      | "++" UnaryExpression | "--" UnaryExpression | "+" UnaryExpression | "-" UnaryExpression
        //                      | "~" UnaryExpresssion | "!" UnaryExpression 
        // UnaryExpression = PostfixExpression | delete UnaryExpression | void UnaryExpression | typeof UnarryExpression |
        //                      | "++" UnaryExpression | "--" UnaryExpression | "+" UnaryExpression | "-" UnaryExpression
        
        private void parseUnaryExpression()
        {
            if (inFirstOfPostfixExpression())   // sym in first(postfixexpression)
            {
                parsePostfixExpression();
            }
            else if (checkReservedWord("delete"))
            {
                parseReservedWord("delete");
                parseUnaryExpression();
            }
            else if (checkReservedWord("void"))
            {
                parseReservedWord("void");
                parseUnaryExpression();
            }
            else if (checkReservedWord("typeof"))
            {
                parseReservedWord("typeof");
                parseUnaryExpression();
            }
            else if (checkTokenTag(DomainTag.INCREMENT))
            {
                parseTerminal(DomainTag.INCREMENT);
                parseUnaryExpression();
            }
            else if (checkTokenTag(DomainTag.DECREMENT))
            {
                parseTerminal(DomainTag.DECREMENT);
                parseUnaryExpression();
            }
            else if (checkTokenTag(DomainTag.PLUS))
            {
                parseTerminal(DomainTag.PLUS);
                parseUnaryExpression();
            }
            else if (checkTokenTag(DomainTag.MINUS))
            {
                parseTerminal(DomainTag.MINUS);
                parseUnaryExpression();
            }
        }

        // Мультипликативные
        // MultiplicativeExpression = UnaryExpression | MultiplicativeExpression "*" UnaryExpression |
        //                              | MultiplicativeExpression "/" UnaryExpression | MultiplicativeExpression "%" UnaryExpression
        private void parseMultiplicativeExpression()
        {
            if (inFirstOfUnaryExpression()) // sym in first(unaryExpression)
            {
                parseUnaryExpression();
            }
            else
            {
                parseMultiplicativeExpression();
                if (checkTokenTag(DomainTag.MUL))
                {
                    parseTerminal(DomainTag.MUL);
                }
                else if (checkTokenTag(DomainTag.DIV))
                {
                    parseTerminal(DomainTag.DIV);
                }
                else
                {
                    parseTerminal(DomainTag.PERCENT);
                }
                parseUnaryExpression();
            }
        }
        // Аддитивные операторы
        // AdditiveExpression = MultiplicativeExpression | AdditiveExpression "+" MultiplicativeExpression |
        //                          | AdditiveExpression "-" MultiplicativeExpression
        private void parseAdditiveExpression()
        {
            if (inFirstOfMultiplicativeExpression()) // sym in first MultiplicativeExpression
            {
                parseMultiplicativeExpression();
            }
            else
            {
                parseAdditiveExpression();
                if (checkTokenTag(DomainTag.PLUS))
                {
                    parseTerminal(DomainTag.PLUS);
                }
                else
                {
                    parseTerminal(DomainTag.MINUS);
                }
                parseMultiplicativeExpression();
            }
        }
        // Операторы побитового сдвига
        // ShiftExpression = AdditiveExpression | ShiftExpression "<<" AdditiveExrpession 
        //                      | ShiftExpression ">>" AdditiveExpression
        private void parseShiftExpression()
        {
            if (inFirstOfAdditiveExpression()) // sym in first(additiveExpression)
            {
                parseAdditiveExpression();
            }
            else
            {
                parseShiftExpression();
                if (checkTokenTag(DomainTag.LSHIFT))
                {
                    parseTerminal(DomainTag.LSHIFT);
                }
                else
                {
                    parseTerminal(DomainTag.RSHIFT);
                }
                parseAdditiveExpression();
            }
        }
        // Операторы отношения
        // RelationalExpressionNoIn = ShiftExpression | RelationalExpressionNoIn "<" ShiftExpression 
        //                              | RelationalExpressionNoIn ">" ShiftExpression 
        //                              | RelationalExpressionNoIn "<=" ShiftExpression 
        //                              | RelationalExpressionNoIn ">=" ShiftExpression
        //                              | RelationalExpressionNoIn instanceof ShiftExpression (not implemented)
        private void parseRelationExpressionNoIn()
        {
            if (inFirstOfShiftExpression())   // symbol in first (ShiftExpression)
            {
                parseShiftExpression();
            }
            else
            {
                parseRelationExpressionNoIn();
                if (checkTokenTag(DomainTag.LESS))
                {
                    parseTerminal(DomainTag.LESS);
                }
                else if (checkTokenTag(DomainTag.LARGER))
                {
                    parseTerminal(DomainTag.LARGER);
                }
                else if (checkTokenTag(DomainTag.LESS_OR_EQUAL))
                {
                    parseTerminal(DomainTag.LESS_OR_EQUAL);
                }
                else// if (checkTokenTag(DomainTag.LARGER_OR_EQUAL))
                {
                    parseTerminal(DomainTag.LARGER_OR_EQUAL);
                }
            }
        }
        // Операторы равенства
        //  EqualityExpressionNoIn = RelationalExpressionNoIn | EqualityExpressionNoIn "==" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "!=" RelationalExpressionNoIn
        //                              | EqualityExpressionNoIn "===" RelationalExpressionNoIn (not implemented)
        //                              | EqualityExpressionNoIn "!==" RelationalExpressionNoIn (not implemented)
        private void parseEqulityExpressionNoIn()
        {
            if (inFirstOfRelationalExpressionNoIn()) // sym in first(relationalExpressionNoIn)
            {
                parseRelationExpressionNoIn();
            }
            else
            {
                parseEqulityExpressionNoIn();
                if (checkTokenTag(DomainTag.LOGICAL_EQUAL))
                {
                    parseTerminal(DomainTag.LOGICAL_EQUAL);
                }
                else
                {
                    parseTerminal(DomainTag.LOGICAL_NOT_EQUAL);
                }
                parseRelationExpressionNoIn();
            }
        }
        // Бинарные побитовые операции
        // BitwiseANDExpressionNoIn = EqualityExpressionNoIn | BitwiseANDExpressionNoIn "&" EqualityExpressionNoIn
        private void parseBitwiseANDExpressionNoIn()
        {
            if (inFirstOfEqualityExpressionNoIn()) // sym in first EqualiteExpressionNoIN
            {
                parseEqulityExpressionNoIn();
            }
            else
            {
                parseBitwiseANDExpressionNoIn();
                parseTerminal(DomainTag.LOGICAL_AND);
                parseEqulityExpressionNoIn();
            }
        }
        // BitwiseXORExpressionNoIn = BitwiseANDExpressionNoIn | BitwiseXORExpressionNoIn "^" BitwiseANDExpressionNoIn
        private void parseBitwiseXORExpressionNoIn()
        {
            if (inFirstOfEqualityExpressionNoIn())
            {
                parseBitwiseANDExpressionNoIn();
            }
            else
            {
                parseBitwiseXORExpressionNoIn();
                parseTerminal(DomainTag.XOR);
                parseBitwiseANDExpressionNoIn();
            }
        }

        // BitwiseORExpressionNoIn = BitwiseXORExpressionNoIn | BitwiseORExpressionNoIn "|" BitwiseXORExpressionNoIn
        private void parseBitwiseORExpressionNoIn()
        {
            if (inFirstOfBitwiseXORExpressionNoIn()) // sym in first BitwiseXORExpressionNoIn
            {
                parseBitwiseXORExpressionNoIn();
            }
            else
            {
                parseBitwiseORExpressionNoIn();
                parseTerminal(DomainTag.OR);
                parseBitwiseXORExpressionNoIn();
            }
        }
        // Бинарные логические операторы
        // LogicalANDExpressionNoIn = BitwiseORExpressionNoIn | LogicalANDExpressionNoIn "&&" BitwiseORExpressionNoIn 
        private void parseLogicalANDExpressionNoIn()
        {
            if (inFirstOfBitwiseORExpressionNoIn()) // sym in first[bitwiseorexpressionnoin]
            {
                parseBitwiseORExpressionNoIn();
            }
            else
            {
                parseLogicalANDExpressionNoIn();
                parseTerminal(DomainTag.LOGICAL_AND);
                parseBitwiseORExpressionNoIn();
            }
        }
        // LogicalORExpressionNoIn = LogicalANDExpressionNoIn | LogicalORExpressionNoIn "||" LogicalANDExpressionNoIn
        private void parseLogicalORExpressionNoIn()
        {
            if (inFirstOfLogicalANDExpressionNoIn()) // sym in first[LogicalANDExpressionNoIn]
            {
                parseLogicalANDExpressionNoIn();
            }
            else
            {
                parseLogicalORExpressionNoIn();
                parseTerminal(DomainTag.LOGICAL_OR);
                parseLogicalANDExpressionNoIn();
            }
        }
        // Условные оператор ( ? : )
        // ConditionalExpressionNoIn = LogicalORExpressionNoIn 
        //                              | LogicalORExpressionNoIn "?" AssignmentExpression ":" AssignmentExpressionNoIn
        private void parseConditionalExpressionNoIn()
        {
            parseLogicalORExpressionNoIn();
            if (checkTokenTag(DomainTag.QUESTION))
            {
                parseTerminal(DomainTag.QUESTION);
                parseAssignmentExpressionNoIn();
                parseTerminal(DomainTag.COLON);
                parseAssignmentExpressionNoIn();
               
            }
        }
        // AssignmentExpressionNoIn = ConditionalExpressionNoIn | LeftHandSideExpression AssignmentOperator AssignmentExpressionNoIn
        private void parseAssignmentExpressionNoIn()
        {
            if (inFirstOfConditionalExpressionNoIn()) // first in [condit exprs]
            {
                parseConditionalExpressionNoIn();
            }
            else
            {
                parseLeftHandSideExpression();
                // not implement +=, -=, /=, %=...
                parseTerminal(DomainTag.EQUAL);
                parseAssignmentExpressionNoIn();
            }
        }
        // Оператор запятая
        // ExpressionNoIn = AssignmentExpressionNoIn | ExpressionNoIn "," AssignmentExpressionNoIn
        private void parseExpressionNoIn()
        {
            if (inFirstOfAssignmentExpressionNoIn()) // sym in first assginmentNoIn
            {
                parseAssignmentExpressionNoIn();
            }
            else
            {
                parseExpressionNoIn();
                parseTerminal(DomainTag.COMMA);
                parseAssignmentExpressionNoIn();
            }
        }

        
    }
}
