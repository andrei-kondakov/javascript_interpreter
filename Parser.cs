using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    class Parser
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
        private bool inFirstOfStatement()
        {
            // TODO: sym in first(ExpressionStatement)
            return checkTokenTag(DomainTag.LBRACE) || checkReservedWord("var") || checkTokenTag(DomainTag.SEMICOLON) ||
                checkReservedWord("if") || checkReservedWord("for") || checkReservedWord("while") || checkReservedWord("do") ||
                checkReservedWord("break") || checkReservedWord("continue") || checkReservedWord("return") ||
                checkReservedWord("switch");
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
            else if (true) // TODO: sym in first(ExpressionStatement)
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
            parseAssignmentExpression();
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
                parseExpression();
            }
            parseTerminal(DomainTag.SEMICOLON);
        }

        // Инструкция if
        // IfStatement = if "(" Expression ")" Statement [ else Statement ] 
        private void parseIfStatement()
        {
            parseReservedWord("if");
            parseTerminal(DomainTag.LPARENT);
            parseExpression();
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
                parseExpression();
                parseTerminal(DomainTag.RBRACE);
                parseTerminal(DomainTag.SEMICOLON);
            }
            else if (checkReservedWord("while"))
            {
                parseReservedWord("while");
                parseTerminal(DomainTag.LPARENT);
                parseExpression();
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
                        parseExpression();
                    }
                    parseTerminal(DomainTag.SEMICOLON);
                    if (true)  // sym in first(expression)
                    {
                        parseExpression();
                    }
                    parseTerminal(DomainTag.SEMICOLON);
                    if (true)  // sym in first(expression)
                    {
                        parseExpression();
                    }
                }
                else
                {
                    //  | for "(" var VariableDeclarations ";" [Expression] ";" [Expression] ) Statement 
                    parseVariableStatement();
                    if (true)  // sym in first(expression)
                    {
                        parseExpression();
                    }
                    parseTerminal(DomainTag.SEMICOLON);
                    if (true)  // sym in first(expression)
                    {
                        parseExpression();
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
            parseExpression();
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
        // AssignmentExpression = ConditionalExpression | LeftHandSideExpression AssignmentOperator AssignmentExpression
        //TODO
        void parseAssignmentExpression()
        {

        }
        //TODO
        void parseExpression()
        {

        }
    }
}
