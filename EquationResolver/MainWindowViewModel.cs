using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EquationResolver
{
    public partial class MainWindowViewModel : ObservableObject
    {

        #region Private Members

        private byte[] ASCIIvalues;

        private byte[] GetASCIIvalues(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        #endregion EndRegion-Private Members

        #region Observable Public Properties
        /// <summary>
        ///      The original string that is bound to the Equation TextBox
        /// </summary>
        [ObservableProperty]
        private string originalString = "(6482^2) + 999 + 000";

        [ObservableProperty]
        private string trimmedAndDeSpacedOriginalString;

        #endregion EndRegion:  Observable Public Properties




        [ObservableProperty] private Progress progressStatus = Progress.None;

        [ObservableProperty] private double result = double.NaN;

        [ObservableProperty] private bool equationFailed = false;

        [ObservableProperty] private string errorMessage = string.Empty;

        [RelayCommand]
        public void SolveEquationButtonPressed()
        {
            // Set the string being processed to the original string (only necess. if it never changes)
            trimmedAndDeSpacedOriginalString = OriginalString.Replace(" ", "");

            trimmedAndDeSpacedOriginalString = FixForMinus(trimmedAndDeSpacedOriginalString);

            // Create the list of ascii values for the string
            ASCIIvalues = GetASCIIvalues(trimmedAndDeSpacedOriginalString);

            // Check to see if any of the characters don't belong in an equation
            var characterValidationCheck = VerifyAllCharsInStringAreValidEquationCharacters(ASCIIvalues);

            if (!characterValidationCheck) throw new Exception();



            // Notify the Progress that we have pressed the solve button
            ProgressStatus = Progress.SolveButtonHasBeenPressed;

            // Call the EquationStringResolver method
            var equationResult = EquationStringResolver(trimmedAndDeSpacedOriginalString);



            Result = equationResult;
        }

        private string FixForMinus(string trimmedAndDeSpacedOriginalString)
        {
            var bytes = GetASCIIvalues(trimmedAndDeSpacedOriginalString);

            string result = "";

            for (var i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 45)
                {
                    if (i == 0) result = result + trimmedAndDeSpacedOriginalString.Substring(i, 1);
                    else if (IsANumber(bytes[i - 1])) result = result + "+-";
                    else result = result + "-";
                }
                 
                else result = result + trimmedAndDeSpacedOriginalString.Substring(i, 1);
            }

            return result;
        }

        private bool VerifyAllCharsInStringAreValidEquationCharacters(byte[] bytesToCheck)
        {
            foreach (var b in bytesToCheck)
            {

                if (!((b >= 40 && b <= 43) || (b >= 45 && b <= 57) || (b == 94))) return false;
            }

            return true;
        }




        private double EquationStringResolver(string stringToResolve)
        {

            ProgressStatus = Progress.EquationNotSolvedBecauseOfErrorsInTheTextbox;

            var bytes = GetASCIIvalues(stringToResolve);

            for (int i = 0; i < bytes.Length; i++)
            {
                var t = bytes[i];

                // If the character is not 0-9  +  -  /  *  ^  ( or ) fail the equation
                if (!((bytes[i] < 40) || (bytes[i] > 57 && bytes[i] < 94) || (bytes[i] > 94) || bytes[i] != 46 || bytes[i] != 47))
                {

                    FailEquation("invalid character in equation");

                    throw new Exception();
                }

                // If we find an open bracket...(
                if (bytes[i] == 40)
                {
                    // Find the corresponding closing bracket and get the string inside the brackets
                    var result = CutOutTextInsideOfBrackets(i, stringToResolve);

                    // Remember the first part of the string before the opening bracket
                    var firstHalfOfString = stringToResolve.Substring(0, i);

                    // Remember the last part of the string after the corresponding closing bracket...)  
                    var secondHalfOfString = stringToResolve.Substring(i + (result.Length + 2));

                    var x = EquationStringResolver(result);
                }



            }

            return SolveSimlifiedEquation(stringToResolve);


        }

        private double SolveSimlifiedEquation(string stringToResolve)
        {
            var stringrecieved = stringToResolve;

            // List of operators asc  ^  *   /   +   
            int[] operatorsAscii = { 94, 42, 47, 43 };

            foreach (var o in operatorsAscii)
            {
                int i = 0;

                var l = stringrecieved.Length;

                var r = "";
                var firstintstr = "";
                var secondintstr = "";
                bool operatorFound = false;
                while (i <= l)
                {
                    int a = 0;
                    if (i < l)
                    {
                        a = (int)stringrecieved[i];
                        var y = stringrecieved[i];

                    }



                    if (((a == 45) || (a == 46) || (a >= 48 && a <= 57)) && !operatorFound)
                    {
                        firstintstr = firstintstr + stringrecieved.Substring(i, 1);
                        
                    }
                    else if ((a == 45) || (a == 46) || ((a >= 48 && a <= 57)) && operatorFound)
                    {
                        if (!(secondintstr.Length > 0)) { firstintstr = firstintstr.Substring(0, firstintstr.Length - 1); }
                        secondintstr = secondintstr + stringrecieved.Substring(i , 1);
                        
                    }
                    else if ((a == o) && !operatorFound)
                    {
                        operatorFound = true;
                        
                    }


                    else if (operatorFound)
                    {
                        double sol;

                        if (o == 94) sol = Math.Pow(Convert.ToDouble(firstintstr), Convert.ToDouble(secondintstr));

                        if (o == 42) sol = Convert.ToDouble(firstintstr) * Convert.ToDouble(secondintstr);

                        if (o == 47) sol = Convert.ToDouble(firstintstr) / Convert.ToDouble(secondintstr);

                        if (o == 43) sol = Convert.ToDouble(firstintstr) + Convert.ToDouble(secondintstr);
                    }
                    else
                    {
                        firstintstr = secondintstr;
                        secondintstr = "";
                    }
                    i++;
                }

            }

            return 0;

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

    private void FailEquation(string message)
    {
        EquationFailed = true;

        ErrorMessage = message;
    }
}
}
