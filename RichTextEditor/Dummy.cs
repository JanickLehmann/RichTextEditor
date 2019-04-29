namespace RichTextEditor
{
    /*
     * This class is defined for only one reason.
     * This dummy class can be used by referencing applications to force the assembly resolver to copy the DLL to an output directory.
     * Let's say we have the following project structure:
     * - Project A
     * - Project B (referenced by Project A)
     * If this library is now referenced from Project B and Project B will only use the UserControl (TextEditor), it will result in a XamlParseException.
     * The cause is that the DLL is copied to the output directory of Project B, but it will not get copied to the output directory of Project A.
     * That means that during runtime, the DLL is not found therefor resulting in the above exception.
     * 
     * To solve this we can define a dummy class (this class) which can be used by the referencing assembly.
     * Just be sure you are using the Dummy class is used as a public property. If it is not public or is not used the optimizer will remove the code, resulting in the same error.
     * 
     * Source: https://stackoverflow.com/questions/15816769/dependent-dll-is-not-getting-copied-to-the-build-output-folder-in-visual-studio/24828522
     */

    public class Dummy
    {
    }
}
