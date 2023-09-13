using System;
using System.Collections;

namespace Cube.Utility
{
    //
    // 三种方式可自动计算字符串公式的值:
    // 1. 最简单的方式，由SQL语句计算
    // 2. 使用Microsoft.Javascript计算
    // 3. 使用后序表达式计算
    //
    // 如字符串："23+56/(102-100)*((36-24)/(8-6))"，计算结果=191。
    // 把表达式由中序式转换成后序式，再用栈来进行计算。
    // 转换为后序时为："23|56|102|100|-|/|*|36|24|-|8|6|-|/|*|+"(其中字符"|"为分隔符)。
    //
    
    /// <summary>
    /// 简单公式计算，只支持四则运算
    /// </summary>
    public class SimpleArithmetic
    {
        //public enum EnumOpt
        //{
        //    Add,//加号
        //    Dec,//减号
        //    Mul,//乘号
        //    Div,//除号
        //    Sin,//正玄
        //    Cos,//余玄
        //    Tan,//正切
        //    ATan,//余切
        //    Sqrt,//平方根
        //    Pow,//求幂
        //    None,//无
        //}

        /// <summary>
        /// 计算表达式的值
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public string Calculate(string expression)
        {
            ArrayList operatorList = new ArrayList();
            string operator1;
            string ExpressionString = "";
            string operand3;
            expression = expression.Replace(" ", "");
            while (expression.Length > 0)
            {
                operand3 = "";
                //取数字处理
                if (Char.IsNumber(expression[0]))
                {
                    while (Char.IsNumber(expression[0]))
                    {
                        operand3 += expression[0].ToString();
                        expression = expression.Substring(1);
                        if (expression == "") break;
                    }

                    ExpressionString += operand3 + "|";
                }

                //取“(”处理
                if (expression.Length > 0 && expression[0].ToString() == "(")
                {
                    operatorList.Add("(");
                    expression = expression.Substring(1);
                }

                //取“)”处理
                operand3 = "";
                if (expression.Length > 0 && expression[0].ToString() == ")")
                {
                    do
                    {
                        if (operatorList[operatorList.Count - 1].ToString() != "(")
                        {
                            operand3 += operatorList[operatorList.Count - 1].ToString() + "|";
                            operatorList.RemoveAt(operatorList.Count - 1);
                        }
                        else
                        {
                            operatorList.RemoveAt(operatorList.Count - 1);
                            break;
                        }
                    } while (true);

                    ExpressionString += operand3;
                    expression = expression.Substring(1);
                }

                //取运算符号处理
                operand3 = "";
                if (expression.Length > 0 && (expression[0].ToString() == "*" || expression[0].ToString() == "/"
                                                                              || expression[0].ToString() == "+" || expression[0].ToString() == "-"))
                {
                    operator1 = expression[0].ToString();
                    if (operatorList.Count > 0)
                    {
                        if (operatorList[operatorList.Count - 1].ToString() == "("
                            || OperatorPriority(operator1, operatorList[operatorList.Count - 1].ToString()))
                        {
                            operatorList.Add(operator1);
                        }
                        else
                        {
                            operand3 += operatorList[operatorList.Count - 1].ToString() + "|";
                            operatorList.RemoveAt(operatorList.Count - 1);
                            operatorList.Add(operator1);
                            ExpressionString += operand3;
                        }
                    }
                    else
                    {
                        operatorList.Add(operator1);
                    }

                    expression = expression.Substring(1);
                }
            }

            operand3 = "";
            while (operatorList.Count != 0)
            {
                operand3 += operatorList[operatorList.Count - 1].ToString() + "|";
                operatorList.RemoveAt(operatorList.Count - 1);
            }

            ExpressionString += operand3.Substring(0, operand3.Length - 1);
            ;

            return CalculateParenthesesExpressionEx(ExpressionString);
        }

        // 第二步:把转换成后序表达的式子计算
        //23|56|102|100|-|/|*|36|24|-|8|6|-|/|*|+"
        private string CalculateParenthesesExpressionEx(string expression)
        {
            //定义两个栈，这里使用数组模拟栈
            ArrayList operandList = new ArrayList();
            float operand1;
            float operand2;
            string[] operand3;

            expression = expression.Replace(" ", "");
            operand3 = expression.Split(Convert.ToChar("|"));

            for (int i = 0; i < operand3.Length; i++)
            {
                if (Char.IsNumber(operand3[i], 0))
                {
                    operandList.Add(operand3[i].ToString());
                }
                else
                {
                    //两个操作数退栈和一个操作符退栈计算
                    operand2 = (float)Convert.ToDouble(operandList[operandList.Count - 1]);
                    operandList.RemoveAt(operandList.Count - 1);
                    operand1 = (float)Convert.ToDouble(operandList[operandList.Count - 1]);
                    operandList.RemoveAt(operandList.Count - 1);
                    operandList.Add(Calculate(operand1, operand2, operand3[i]).ToString());
                }
            }

            return operandList[0].ToString();
        }

        //判断两个运算符优先级别
        private bool OperatorPriority(string Operator1, string Operator2)
        {
            if (Operator1 == "*" && Operator2 == "+")
                return true;
            else if (Operator1 == "*" && Operator2 == "-")
                return true;
            else if (Operator1 == "/" && Operator2 == "+")
                return true;
            else if (Operator1 == "/" && Operator2 == "-")
                return true;
            else
                return false;
        }

        //计算
        private float Calculate(float operand1, float operand2, string operator2)
        {
            switch (operator2)
            {
                case "*":
                    operand1 *= operand2;
                    break;
                case "/":
                    operand1 /= operand2;
                    break;
                case "+":
                    operand1 += operand2;
                    break;
                case "-":
                    operand1 -= operand2;
                    break;
                default:
                    break;
            }

            return operand1;
        }
    }
}