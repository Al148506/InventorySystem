using System.Reflection;
using System.Runtime.Loader;
namespace InventorySystem.CommonLib
{
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException($"La biblioteca no se encontró en la ruta: {absolutePath}");
            }
                return LoadUnmanagedDllFromPath(absolutePath);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = Path.Combine(AppContext.BaseDirectory, "lib",
                Environment.Is64BitProcess ? "libwkhtmltox.dll" : "libwkhtmltox32.dll");

            return LoadUnmanagedDllFromPath(libraryPath);
        }
    }

}
