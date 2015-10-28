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
            // TODO: sym in first(ExpressionStatement)
            return checkTokenTag(DomainTag.LBRACE) || checkReservedWord("var") || checkTokenTag(DomainTag.SEMICOLON) ||
                checkReservedWord("if") || checkReservedWord("for") || checkReservedWord("while") || checkReservedWord("do") ||
                checkReservedWord("break") || checkReservedWord("continue") || checkReservedWord("return") ||
                checkReservedWord("switch");
        }
        // sym in FIRST[ExpressionStatement]
        private bool inFirstOfExpressionStatement()
        {
            return inFirstOfExpressionNoIn();
        }
        // sym in FIRST[Literal]
        private bool inFirstOfLiteral()
        {
            return checkReservedWord("null") || checkReservedWord("true") || checkReservedWord("false") ||
                checkTokenTag(DomainTag.NUMBER) || checkTokenTag(DomainTag.STRING);
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
            return inFirstOfPrimaryExpression() || checkReservedWord("function") || checkReservedWord("new"); // TEST!
        }
        // sym in FIRST[CallExpression]
        private bool inFirstOfCallExpression()
        {
            return inFirstOfMemberExpression(); // QUESTION!!
        }
        // sym in FIRST[NewExpression]
        private bool inFirstOfNewExpression()
        {
            return inFirstOfMemberExpression() || checkReservedWord("new");
        }
        // sym in FIRST[LeftHandSideExpression]
        private bool inFirstOfLeftHandSideExpression()
        {
            return inFirstOfNewExpression() || inFirstOfCallExpression();
        }
        // sym in FIRST[PostfixExpression]
        private bool inFirstOfPostfixExpression()
        {
            return inFirstOfLeftHandSideExpression();
        }
        // sym in FIRST[UnaryExpression] 
        private bool inFirstOfUnaryExpression()
        {
            return inFirstOfPostfixExpression() || checkReservedWord("delete") || checkReservedWord("void") || checkReservedWord("typeof")
                || checkTokenTag(DomainTag.INCREMENT) || checkTokenTag(DomainTag.DECREMENT) || checkTokenTag(DomainTag.PLUS)
                || checkTokenTag(DomainTag.MINUS);
        }
        // sym in FIRST[MultiplicativeExpression] 
        private bool inFirstOfMultiplicativeExpression()
        {
            return inFirstOfUnaryExpression(); // TEST
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
        private bool inFirstOfRelationalExpressionNoIn()
        {
            return inFirstOfShiftExpression();
        }
        // sym in FIRST[EqualityExpressionNoIn] 
        private bool inFirstOfEqualityExpressionNoIn()
        {
            return inFirstOfRelationalExpressionNoIn();
        }
        // sym in FIRST[BitwiseANDExpressionNoIn] 
        private bool inFirstOfBitwiseANDExpressionNoIn()
        {
            return inFirstOfEqualityExpressionNoIn();
        }
        // sym in FIRST[BitwiseXORExpressionNoIn] 
        private bool inFirstOfBitwiseXORExpressionNoIn()
        {
            return inFirstOfBitwiseANDExpressionNoIn();
        }
        // sym in FIRST[BitwiseXORExpressionNoIn] 
        private bool inFirstOfBitwiseORExpressionNoIn()
        {
            return inFirstOfBitwiseXORExpressionNoIn();
        }
        // sym in FIRST[LogicalANDExpressionNoIn]
        private bool inFirstOfLogicalANDExpressionNoIn()
        {
            return inFirstOfBitwiseORExpressionNoIn();
        }
        // sym in FIRST[LogicalORExpressionNoIn]
        private bool inFirstOfLogicalORExpressionNoIn()
        {
            return inFirstOfLogicalANDExpressionNoIn();
        }
        // sym in FIRST[ConditionalExpressionNoIn]
        private bool inFirstOfConditionalExpressionNoIn()
        {
            return inFirstOfLogicalORExpressionNoIn();
        }
        // sym in FIRST[AssignmentExpressionNoIn]
        private bool inFirstOfAssignmentExpressionNoIn()
        {
            return inFirstOfConditionalExpressionNoIn() || inFirstOfLeftHandSideExpression();
        }
        // sym in FIRST[ExpressionNoIn]
        private bool inFirstOfExpressionNoIn()
        {
            return inFirstOfAssignmentExpressionNoIn();
        }
    }
}
