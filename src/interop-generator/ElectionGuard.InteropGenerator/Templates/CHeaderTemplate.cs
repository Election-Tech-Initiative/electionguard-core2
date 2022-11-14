﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace ElectionGuard.InteropGenerator.Templates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using ElectionGuard.InteropGenerator.Helpers;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class CHeaderTemplate : CHeaderTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            
            #line 7 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"

	var classNameSnakeCase = EgClass.ClassName.ToSnakeCase();
	var classHandle = $"eg_{classNameSnakeCase}_t";

            
            #line default
            #line hidden
            this.Write("/// @file ");
            
            #line 11 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(classNameSnakeCase.ToLower()));
            
            #line default
            #line hidden
            this.Write(".generated.h\r\n#pragma once\r\n\r\n#include \"chaum_pedersen.h\"\r\n#include \"elgamal.h\"\r\n" +
                    "#include \"export.h\"\r\n#include \"group.h\"\r\n#include \"status.h\"\r\n#include \"ballot.h" +
                    "\"\r\n\r\n#ifdef __cplusplus\r\nextern \"C\" {\r\n#endif\r\n\r\n#ifndef ");
            
            #line 25 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(EgClass.ClassName));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n");
            
            #line 27 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 foreach (var egProperty in EgClass.Properties) { 
	var entryPoint = egProperty.GetEntryPoint(EgClass.ClassName);
	var returnType = egProperty.GetCReturnType();
	var outParamName = "out_" + egProperty.Name.ToSnakeCase();

            
            #line default
            #line hidden
            this.Write("/**\r\n * @brief ");
            
            #line 33 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(egProperty.Description));
            
            #line default
            #line hidden
            this.Write("\r\n * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque " +
                    "instance\r\n");
            
            #line 35 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 if (egProperty.IsReferenceType()) { 
            
            #line default
            #line hidden
            this.Write(" * @param[out] ");
            
            #line 36 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(outParamName));
            
            #line default
            #line hidden
            this.Write(" A pointer to the output ");
            
            #line 36 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(egProperty.Name));
            
            #line default
            #line hidden
            this.Write(".  ");
            
            #line 36 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"

if (egProperty.CallerShouldFree) {
	this.Write("The caller is responsible for freeing it.");
} else {
	this.Write("The value is a reference and is not owned by the caller.");
}
            
            #line default
            #line hidden
            this.Write("\r\n * @return eg_electionguard_status_t indicating success or failure\r\n * @retval " +
                    "ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed\r\n * @retval " +
                    "ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory\r" +
                    "\n");
            
            #line 46 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } else { 
            
            #line default
            #line hidden
            this.Write(" * @return The value of the property\r\n");
            
            #line 48 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } // end is reference type 
            
            #line default
            #line hidden
            this.Write(" */\r\nEG_API ");
            
            #line 50 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(returnType));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 50 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(entryPoint));
            
            #line default
            #line hidden
            this.Write("(\r\n\t");
            
            #line 51 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(classHandle));
            
            #line default
            #line hidden
            this.Write(" *handle");
            
            #line 51 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"

if (egProperty.IsReferenceType()) {

            
            #line default
            #line hidden
            this.Write(",\r\n\t");
            
            #line 54 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(egProperty.OutVarType));
            
            #line default
            #line hidden
            
            #line 54 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(outParamName));
            
            #line default
            #line hidden
            this.Write("\r\n\t);\r\n");
            
            #line 56 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } else { 
            
            #line default
            #line hidden
            this.Write("\r\n\t);\r\n");
            
            #line 59 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 61 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } // foreach property 
            
            #line default
            #line hidden
            
            #line 62 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 foreach (var egMethod in EgClass.Methods) { 
	var entryPoint = egMethod.GetEntryPoint(EgClass.ClassName); 
	var returnType = egMethod.GetCReturnType();
	
            
            #line default
            #line hidden
            this.Write("/**\r\n * ");
            
            #line 67 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(egMethod.Description));
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 68 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 foreach (var parameter in egMethod.Params.Where(p => p.Description != null)) { 
            
            #line default
            #line hidden
            this.Write(" * @param[in] ");
            
            #line 69 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(parameter.CName));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 69 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(parameter.Description));
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 70 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } // foreach parameter 
            
            #line default
            #line hidden
            
            #line 71 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 if (egMethod.ReturnType.IsReferenceType) { 
            
            #line default
            #line hidden
            this.Write(" * @param[out] ");
            
            #line 72 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(egMethod.ReturnTypeCName));
            
            #line default
            #line hidden
            this.Write(" An opaque pointer to the ");
            
            #line 72 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(egMethod.ReturnType.TypeCs));
            
            #line default
            #line hidden
            this.Write("\r\n *                               The value is a reference and is not owned by t" +
                    "he caller\r\n");
            
            #line 74 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } // end if method is reference type 
            
            #line default
            #line hidden
            this.Write(" */\r\nEG_API ");
            
            #line 76 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(returnType));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 76 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(entryPoint));
            
            #line default
            #line hidden
            this.Write("(\r\n\teg_");
            
            #line 77 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(EgClass.ClassName.ToSnakeCase()));
            
            #line default
            #line hidden
            this.Write("_t *handle");
            
            #line 77 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"

foreach (var parameter in egMethod.Params) {
	var parameterCName = parameter.CName;
	this.Write($",{Environment.NewLine}\t{parameter.TypeC}{parameterCName}");
}
if (egMethod.ReturnType.IsReferenceType) {
	var outVarReturnType = egMethod.ReturnType.OutVarCType;
	var outParamName = egMethod.ReturnTypeCName;
	this.Write($",{Environment.NewLine}\t{outVarReturnType}{outParamName}");
}

            
            #line default
            #line hidden
            this.Write("\r\n\t);\r\n\r\n");
            
            #line 91 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
 } // foreach method 
            
            #line default
            #line hidden
            this.Write("EG_API eg_electionguard_status_t eg_");
            
            #line 92 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(classNameSnakeCase));
            
            #line default
            #line hidden
            this.Write("_free(eg_");
            
            #line 92 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(classNameSnakeCase));
            
            #line default
            #line hidden
            this.Write("_t *handle);\r\n\r\n#endif // ifndef ");
            
            #line 94 "C:\dev\ElectionGuard\electionguard-core2\src\interop-generator\ElectionGuard.InteropGenerator\Templates\CHeaderTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(EgClass.ClassName));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n#ifdef __cplusplus\r\n}\r\n#endif\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public class CHeaderTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
