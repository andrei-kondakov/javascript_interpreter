using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    partial class Parser
    {
        // sym in FIRST[Statement]
        private bool inFirstOfStatement()
        {
            return checkTokenTag(DomainTag.LBRACE) || checkReservedWord("var") || checkTokenTag(DomainTag.SEMICOLON) ||
                checkReservedWord("if") || checkReservedWord("for") || checkReservedWord("while") || checkReservedWord("do") ||
                checkReservedWord("break") || checkReservedWord("continue") || checkReservedWord("return") ||
                checkReservedWord("switch") || inFirstOfExpressionStatement();
        }
        // sym in FIRST[ExpressionStatement]
        private bool inFirstOfExpressionStatement()
        {
            // TEST
            return inFirstOfExpression() &&  !(checkTokenTag(DomainTag.LBRACE) || checkReservedWord("function"));
        }
        // sym in FIRST[Literal]
        private bool inFirstOfLiteral()
        {
            return checkReservedWord("null") || checkReservedWord("true") || checkReservedWord("false") ||
                checkTokenTag(DomainTag.STRING) || checkTokenTag(DomainTag.NUMBER);
        }
        // sym in FIRST[ObjectLiteral]
        private bool inFirstOfObjectLiteral()
        {
            return checkTokenTag(DomainTag.LBRACE);
        }
        // sym in FIRST[PrimaryExpression]
        private bool inFirstOfPrimaryExpression()
        {
            return checkReservedWord("this") || checkTokenTag(DomainTag.IDENT) ||
                 inFirstOfLiteral() || checkTokenTag(DomainTag.LBRACE) || checkTokenTag(DomainTag.LPARENT);
        }
        // sym in FIRST[PropertyAssignmnet]
        private bool inFirstOfPropertyAssignmnet()
        {
            return checkTokenTag(DomainTag.IDENT) ||
                checkTokenTag(DomainTag.STRING) ||
                checkReservedWord("get") ||
                checkReservedWord("set"); 
        }
        // sym in FIRST[PropertyNamesAndValues]
        private bool inFirstOfPropertyNamesAndValues()
        {
            return inFirstOfPropertyAssignmnet();
        }
        // sym in FIRST[PropertyName]
        private bool inFirstOfProperyName()
        {
            return checkTokenTag(DomainTag.IDENT) || checkTokenTag(DomainTag.STRING);
        }
        // sym in FIRST[MemberExpression]
        private bool inFirstOfMemberExpression()
        {
            return inFirstOfPrimaryExpression() || checkReservedWord("function");
        }
        // sym in First[ArgumentList] 
        private bool inFirstOfArgumentList()
        {
            return inFirstOfAssignmentExpression();
        }
        // sym in FIRST[UnaryExpression] 
        private bool inFirstOfUnaryExpression()
        {
            return inFirstOfMemberExpression() || checkTokenTag(DomainTag.PLUS)
                || checkTokenTag(DomainTag.MINUS) || checkTokenTag(DomainTag.INCREMENT) || checkTokenTag(DomainTag.DECREMENT)
                || checkReservedWord("new") || checkReservedWord("delete") || checkTokenTag(DomainTag.LOGICAL_NOT);
        }
        // sym in FIRST[MultiplicativeExpression] 
        private bool inFirstOfMultiplicativeExpression()
        {
            return inFirstOfUnaryExpression();
        }
        // sym in FIRST[AdditiveExpression] 
        private bool inFirstOfAdditiveExpression()
        {
            return inFirstOfMultiplicativeExpression(); // TEST
        }
        // sym in FIRST[ShiftExpression] 
        private bool inFirstOfShiftExpression()
        {
            return inFirstOfAdditiveExpression();
        }
        // sym in FIRST[RelationalExpressionNoIn] 
        private bool inFirstOfRelationalExpression()
        {
            return inFirstOfShiftExpression();
        }
        // sym in FIRST[EqualityExpressionNoIn] 
        private bool inFirstOfEqualityExpression()
        {
            return inFirstOfRelationalExpression();
        }
        // sym in FIRST[BitwiseAndExpression] 
        private bool inFirstOfBitwiseAndExpression()
        {
            return inFirstOfEqualityExpression();
        }
        // sym in FIRST[BitwiseXorExpressionNoIn] 
        private bool inFirstOfBitwiseXorExpression()
        {
            return inFirstOfBitwiseAndExpression();
        }
        // sym in FIRST[BitwiseXorExpression] 
        private bool inFirstOfBitwiseOrExpression()
        {
            return inFirstOfBitwiseXorExpression();
        }
        // sym in FIRST[LogicalAndExpression]
        private bool inFirstOfLogicalAndExpression()
        {
            return inFirstOfBitwiseOrExpression();
        }
        // sym in FIRST[LogicalOrExpression]
        private bool inFirstOfLogicalOrExpression()
        {
            return inFirstOfLogicalAndExpression();
        }
        // sym in FIRST[ConditionalExpression]
        private bool inFirstOfConditionalExpression()
        {
            return inFirstOfLogicalOrExpression();
        }
        // sym in FIRST[AssignmentExpression]
        private bool inFirstOfAssignmentExpression()
        {
            return inFirstOfConditionalExpression();
        }
        // sym in FIRST[Expression]
        private bool inFirstOfExpression()
        {
            return inFirstOfAssignmentExpression();
        }
    }
}
