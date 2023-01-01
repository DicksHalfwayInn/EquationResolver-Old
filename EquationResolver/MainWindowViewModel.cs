using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace EquationResolver
{
    public partial class MainWindowViewModel : ObservableObject
    {

        #region Private Members

        /// <summary>
        ///      The array of bytes
        /// </summary>
        private byte[]? ASCIIvalues;

        /// <summary>
        ///      The original string after removing all the spaces
        /// </summary>
        private string? trimmedAndDeSpacedOriginalString;

        private bool firstPass = true;

        private int calculationCounter = 0;

        #endregion EndRegion-Private Members


        #region Observable Public Properties

        /// <summary>
        ///      The original string that is bound to the Equation TextBox
        /// </summary>
        #region Full Property : OriginalString
        private string originalString = "(1 + 6^2 /3) * 99 - 456";

        public string OriginalString
        {
            get => originalString;
            set
            {
                if (originalString == value) return;

                SetProperty(ref originalString, value);
            }
        }
        #endregion End Region: Full Property : OriginalString

        /// <summary>
        ///      The result of the equation that is bound to the view's Result
        /// </summary>
        [ObservableProperty] private double result = double.NaN;

        /// <summary>
        ///      The Error Message in the view when invalid chars are in the equation
        /// </summary>
        [ObservableProperty] private string invalidCharErrorMessage = "Invalid Characters in Equation";

        /// <summary>
        ///      The Error Message in the view when the brackets don't match up
        ///           re:  Even number of opening and closing brackets
        /// </summary>
        [ObservableProperty] private string mismatchedBracketsErrorMessage = "Open/Close Brackets Mismatched";

        [ObservableProperty] private ObservableCollection<string> calculations = new ObservableCollection<string>();

        /// <summary>
        ///      Flag indicating if the equation has invalid chars in it
        /// </summary>
        private bool invalidCharFound = false;

        public bool InvalidCharFound
        {
            get=> invalidCharFound;
            set
            {
                invalidCharFound = value;
            }
        }



        //[ObservableProperty] private bool invalidCharFound = true;



        /// <summary>
        ///      Flag indicating if there are not an equal number of opening and closing brackets
        /// </summary>
        [ObservableProperty] private bool mismatchedBracketsFound = false;

        #endregion EndRegion:  Observable Public Properties









        /// <summary>
        ///      Button Command:  Runs when the Solve Equation button is pressed
        /// </summary>
        [RelayCommand]
        public void SolveEquationButtonPressed()
        {

            // Remove all the spaces from the OriginalString
            trimmedAndDeSpacedOriginalString = OriginalString.Replace(" ", "");

            // Fix the minuses in the string
            trimmedAndDeSpacedOriginalString = FixForMinus(trimmedAndDeSpacedOriginalString);

            // Create the list of ascii values for the string
            ASCIIvalues = GetASCIIvalues(trimmedAndDeSpacedOriginalString);

            // Check to see if any of the characters don't belong in an equation
            var allCharsAreValid = VerifyAllCharsInStringAreValidEquationCharacters(trimmedAndDeSpacedOriginalString);

            

            if (!allCharsAreValid) return;

            if (MismatchedBracketsFound) return;


            // Call the EquationStringResolver method
            var equationResult = EquationStringResolver(trimmedAndDeSpacedOriginalString);

            Result = Convert.ToDouble(equationResult);

        }

        private void CheckForMismatchedBrackets(string s)
        {
            int bracketsUnmatched = 0;

            foreach (var c in s)
            {
                if ((int)c == 40) bracketsUnmatched++;
                if ((int)c == 41) bracketsUnmatched--;
            }

            if (bracketsUnmatched == 0)
            {
                MismatchedBracketsFound = false;
                return;
            }
            else
            {
                MismatchedBracketsFound = true;
            }
        }

        // Default Constructor
        public MainWindowViewModel()
        {

        }

        public void TextChanged(string s)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                var t = VerifyAllCharsInStringAreValidEquationCharacters(s);
                CheckForMismatchedBrackets(s);
            }
   

            Result = double.NaN;
        }

        /// <summary>
        /// Returns an array of byte for the passed in string parameter
        /// </summary>
        /// <param name="str">The string to convert to bytes array</param>
        /// <returns></returns>
        private byte[] GetASCIIvalues(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        /// <summary>
        ///      Fixes the original string for all the minuses in the original string
        /// </summary>
        /// <param name="trimmedAndDeSpacedOriginalString">The string passed in</param>
        /// <returns></returns>
        private string FixForMinus(string trimmedAndDeSpacedOriginalString)
        {
            // Get an array of bytes for the passed in string
            var bytes = GetASCIIvalues(trimmedAndDeSpacedOriginalString);

            // Declare a property to return as a result
            string result = string.Empty;

            // Iterate through the bytes in the string
            for (var i = 0; i < bytes.Length; i++)
            {
                // If the current byte is a minus (45)
                if (bytes[i] == 45)
                {
                    // If the minus appears at the very beginning of the string don't change anything
                    //      just leave it as the first character
                    if (i == 0) result = result + trimmedAndDeSpacedOriginalString.Substring(i, 1);

                    // If the character before the minus is a number then change it to +- basically,
                    //      change it to adding a negative number
                    else if (IsANumber(bytes[i - 1]) || bytes[i - 1] == 41) result = result + "+-";

                    // If the character before the minus is an operator just leave it alone
                    else result = result + "-";
                }

                // The current byte is not a minus so just add the current character to the result
                else result = result + trimmedAndDeSpacedOriginalString.Substring(i, 1);
            }
            // Return the string with the minuses fixed
            return result;
        }

        /// <summary>
        ///      Check to see if any characters are not numbers or operators, 
        /// </summary>
        /// <param name="bytesToCheck"></param>
        /// <returns></returns>
        private bool VerifyAllCharsInStringAreValidEquationCharacters(string equationString)
        {
            var foundBadChar = false;

            var trimmedAndDeSpacedstring = equationString.Replace(" ", "");
            //
            var bytesToCheck = GetASCIIvalues(trimmedAndDeSpacedstring);

            // Iterate through the bytes in the string to check to see if they belong
            foreach (var b in bytesToCheck)
            {

                var x = (char)b;
                // If you find one that doesn't belong, quit and return false
                if (!((b >= 40 && b <= 43) || (b >= 45 && b <= 57) || (b == 94)))
                {
                    //invalidCharErrorMessage = "Invalid Characters in Equation";
                    foundBadChar = true;

                }

                //// Either the one above or this is correct?
                //if (!((b < 40) || (b > 57 && b < 94) || (b > 94) || b != 46 || b != 47))
                //{
                //    ErrorMessage = "Invalid Characters in Equation";
                //    errorFound = true;
                    
                //}

            }
            if (foundBadChar)
            {
                InvalidCharFound = true;
                
                return false;
            }
            else
            {
                InvalidCharFound = false;
                return true;
            }
        }



        /// <summary>
        ///      The main resolver method start
        /// </summary>
        /// <param name="stringToResolve">The equation to resolve for</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string EquationStringResolver(string stringToResolve)
        {
            
            // Convert the string into an array of bytes
            var bytes = GetASCIIvalues(stringToResolve);

            // Counter for iterating through the characters
            var i = 0;

            // The length of the passed in string
            var l = bytes.Length;

            // The resultant string that includes the text before an opening bracket, the text of the solved
            //      equation, and the text after the corresponding closing bracket
            var resultantString = stringToResolve;

            // Iterate through the bytes
            while (i < l)
            {

                // If we find an open bracket...(
                if ((int)resultantString[i] == 40)
                {
                    // Find the corresponding closing bracket and get the string inside the brackets
                    var cutOutTextFromBrackets = CutOutTextInsideOfBrackets(i, resultantString);

                    // Remember the first part of the string before the opening bracket
                    var firstHalfOfString = resultantString.Substring(0, i);

                    // Remember the last part of the string after the corresponding closing bracket...)  
                    var secondHalfOfString = resultantString.Substring(i + (cutOutTextFromBrackets.Length + 2));

                    // Recursively throw the cut out string back into this method to solve further
                    var solution = EquationStringResolver(cutOutTextFromBrackets);

                    // After the Recursive method has exited...
                    resultantString = firstHalfOfString + solution.ToString() + secondHalfOfString;

                    i = firstHalfOfString.Length + solution.ToString().Length - 1;

                    l = resultantString.Length;
                }
                i++;
            }

            //if (resultantString.Length == 0)
            //{
            //    var answer = SolveSimlifiedEquation(stringToResolve);

            //}
            //else
            //{
            //    throw new Exception();
            //}


            var solvedEquation = SolveSimlifiedEquation(resultantString);

            return solvedEquation;
        }

        /// <summary>
        ///      Solves equations that don't have any brackets
        /// </summary>
        /// <param name="stringToResolve">The string to solve for</param>
        /// <returns></returns>
        private string SolveSimlifiedEquation(string stringToResolve)
        {
            // Working copy of the string to resolve
            var stringrecieved = stringToResolve;

            // Array of operators   ^  *   /   +   
            int[] operatorsAscii = { 94, 42, 47, 43 };

            // Iterate through each of the operators that the text may contain in operator order
            //     powers first, * and / next, then + and -
            foreach (var o in operatorsAscii)
            {
                // The length of the passed in string
                var l = stringrecieved.Length;

                // The string representing the number before the operand found
                var firstNumberString = string.Empty;

                // The start position of the number before the operator in relation to the string in whole
                int firstNumberPosition = 0;

                // The string representing the number after the operand found
                var secondNumberString = string.Empty;

                // The start position of the number after the operator in relation to the string in whole
                int secondintstrFinalPosition = 0;

                // Flag indicating if the currently being iterated through operator was found
                bool operatorFound = false;

                // counter for the while loop
                int i = 0;


                while (i <= l)
                {
                    // default byte value of the char in the string received
                    int a = 0;
                    // If counter is less than the length of the string received
                    if (i < l)
                    {
                        // The byte value of the char being looked at in the received string
                        a = (int)stringrecieved[i];
                    }

                    // If the char is part of a number and we haven't yet found an operator,
                    // then start remembering the number before the next operator is found
                    if (((a == 45) || (a == 46) || (a >= 48 && a <= 57)) && !operatorFound)
                    {
                        // If the first number string hasn't been changed yet, then remember the position
                        if (firstNumberString == string.Empty) firstNumberPosition = i;

                        // Add the char to the first number string
                        firstNumberString = firstNumberString + stringrecieved.Substring(i, 1);
                    }

                    // If the char is part of a number and we have already found an operator then
                    //    add the char to the second number string
                    else if ((a == 45) || (a == 46) || ((a >= 48 && a <= 57)) && operatorFound)
                    {

                        // Add the char to the second number string
                        secondNumberString = secondNumberString + stringrecieved.Substring(i, 1);

                        // Keep track of the final position of the Second Number String
                        secondintstrFinalPosition = i;
                    }

                    // If the char matches the operator being looked at and we haven't found an operator yet...
                    else if ((a == o) && !operatorFound)
                    {
                        // Set the Operator Found flag to true
                        operatorFound = true;
                    }

                    // If we have already found an operator and now we have found another... solve the equation
                    else if (operatorFound)
                    {
                        //  Sample Equation:  (1 + 1^2 *3) * 99 - 003

                        // Declare a variable to keep track of the math solution
                        double solution = 0.0;

                        // If the operator is a Power...
                        if (o == 94) solution = Math.Pow(Convert.ToDouble(firstNumberString), Convert.ToDouble(secondNumberString));

                        // If the operator is Multiplication...
                        if (o == 42) solution = Convert.ToDouble(firstNumberString) * Convert.ToDouble(secondNumberString);

                        // If the operator is Division...
                        if (o == 47) solution = Convert.ToDouble(firstNumberString) / Convert.ToDouble(secondNumberString);

                        // If the operator is Addition
                        if (o == 43) solution = Convert.ToDouble(firstNumberString) + Convert.ToDouble(secondNumberString);

                        calculationCounter++;

                        Calculations.Add(string.Format("EQ # {4}: {0} {1} {2} = {3}", firstNumberString,(char)o, secondNumberString, solution.ToString(), calculationCounter));

                        
                        // Grab the first half of the string before the equation being solved
                        var firstHalfOfString = stringrecieved.Substring(0, firstNumberPosition);

                        // Grab the second half of the string after the equation being solved
                        var secondHalfOfString = (i < l) ?
                            stringrecieved.Substring(secondintstrFinalPosition + 1, stringrecieved.Length - (secondintstrFinalPosition + 1)) : "";

                        // The double solution as a string
                        var solutionString = solution.ToString("#.##");

                        // Change the original string to first half, solution and send half
                        stringrecieved = firstHalfOfString + solutionString + secondHalfOfString;

                        // Reset the counter considering the string recieved has now been changed
                        i = firstNumberPosition + solutionString.Length - 1;

                        // Reset the length of the string recieved now that it has changed
                        l = l + solutionString.Length - firstNumberString.Length - secondNumberString.Length - 1;

                        // reset the Second Number string back to empty
                        secondNumberString = string.Empty;

                        // Set the first Number string to the solution of the equation
                        firstNumberString = solutionString;

                        // Reset the operator found flag back to false
                        operatorFound = false;
                    }
                    // If an operator was found but it wasn't the one being looked at, then change
                    //      the first number found to the second number string and second = empty
                    else
                    {
                        // Reset the First Number string
                        firstNumberString = secondNumberString;

                        // Set the Second Number string to empty
                        secondNumberString = string.Empty;
                    }

                    // Increment the counter
                    i++;
                }

            }

            return stringrecieved;

        }








        private string FindNumberBeforeOperatorAsString(string stringRecieved, int index)
        {


            var firstHalfOfString = stringRecieved.Substring(0, index);

            var i = index;

            foreach (var c in stringRecieved.Substring(0, index).Reverse())
            {
                if (!IsANumber(c))
                {
                    var x = stringRecieved.Substring(i, index - i + 1);

                    return x;
                }
                i--;
            }

            var result = stringRecieved.Substring(0, index);

            return result;

        }



        private string FindNumberAfterOperatorAsString(string stringRecieved, int index)
        {


            var secondHalfOfString = stringRecieved.Substring(index);

            var i = index;

            foreach (var c in secondHalfOfString)
            {
                if (!IsANumber(c))
                {
                    var x = stringRecieved.Substring(i, index - i + 1);

                    return x;
                }
                i--;
            }

            var result = secondHalfOfString;

            return result;

        }





        private int FindEndPositionOfNumberAfterOperator(string stringrecieved, int index)
        {
            var i = index;

            foreach (var c in stringrecieved.Substring(index))
            {
                if (!IsANumber(c)) return i;
                i--;
            }
            return stringrecieved.Length;

        }




        private bool IsANumber(int charAscii)
        {
            if (((charAscii < 48) || (charAscii > 57)) && charAscii != 46) return false;
            else return true;
        }

        private string CutOutTextInsideOfBrackets(int openBracketPosition, string stringToCutBracketsOutOf)
        {
            string result = null;
            int numberOfOpenBrackets = 0;

            var bytes = GetASCIIvalues(stringToCutBracketsOutOf);

            for (int i = openBracketPosition + 1; i < stringToCutBracketsOutOf.Length; i++)
            {

                var cccc = i;

                if (bytes[i] == 40) numberOfOpenBrackets++;
                if (bytes[i] == 41)
                {
                    
                    if (numberOfOpenBrackets == 0)
                    {
                        result = stringToCutBracketsOutOf.Substring(openBracketPosition + 1, i - openBracketPosition - 1);
                    }
                    numberOfOpenBrackets--;
                }

                if (result != null) break;
            }

            if (result != null) { return result; }
            else
            {
                throw new Exception();
                return string.Empty;
            }
        }
    }
}
