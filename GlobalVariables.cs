namespace istWebAPI_ImageRec
{
    public static class GlobalVariables
    {
        public static string pythonHome = @"C:\Users\Administrator\AppData\Local\Programs\Python\Python310";
      // public static string pythonHome = @"C:\Users\hp\AppData\Local\Programs\Python\Python310"; //Haneesh
        // public static string pythonHome = @"C:\Users\Mubarak Ali\AppData\Local\Programs\Python\Python312";   // Mubarak
       // public static string pythonHome = @"C:\Users\Administrator\AppData\Local\Programs\Python\Python311"; //python Server
        public static string pythonPath = "C:/my_python_scripts;" + pythonHome + "/Lib/site-packages";
       public static string pythonDll = pythonHome + "/python310.dll";
       public static string pythonExePath = pythonHome + "/python.exe"; //py.exe  or python.exe Ensure this path is correct
       public static string ImageStorageRootLocation = "c:/";
    }
}
