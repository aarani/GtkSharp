using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

class GLibrary
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    private static Dictionary<Library, IntPtr> _libraries;
    private static Dictionary<string, IntPtr> _customlibraries;
    private static Dictionary <Library, string[]> _libraryDefinitions;

    static GLibrary()
    {
        _customlibraries = new Dictionary<string, IntPtr>();
        _libraries = new Dictionary<Library, IntPtr>();
        _libraryDefinitions = new Dictionary<Library, string[]>();
        _libraryDefinitions[Library.GLib] = new[] { "libglib-2.0-0.dll", "libglib-2.0.so.0", "libglib-2.0.0.dylib", "glib-2.dll" };
        _libraryDefinitions[Library.GObject] = new[] { "libgobject-2.0-0.dll", "libgobject-2.0.so.0", "libgobject-2.0.0.dylib", "gobject-2.dll" };
        _libraryDefinitions[Library.Cairo] = new[] { "libcairo-2.dll", "libcairo.so.2", "libcairo.2.dylib", "cairo.dll" };
        _libraryDefinitions[Library.Gio] = new[] { "libgio-2.0-0.dll", "libgio-2.0.so.0", "libgio-2.0.0.dylib", "gio-2.dll" };
        _libraryDefinitions[Library.Atk] = new[] { "libatk-1.0-0.dll", "libatk-1.0.so.0", "libatk-1.0.0.dylib", "atk-1.dll" };
        _libraryDefinitions[Library.Pango] = new[] { "libpango-1.0-0.dll", "libpango-1.0.so.0", "libpango-1.0.0.dylib", "pango-1.dll" };
        _libraryDefinitions[Library.Gdk] = new[] { "libgdk-3-0.dll", "libgdk-3.so.0", "libgdk-3.0.dylib", "gdk-3.dll" };
        _libraryDefinitions[Library.GdkPixbuf] = new[] { "libgdk_pixbuf-2.0-0.dll", "libgdk_pixbuf-2.0.so.0", "libgdk_pixbuf-2.0.dylib", "gdk_pixbuf-2.dll" };
        _libraryDefinitions[Library.Gtk] = new[] { "libgtk-3-0.dll", "libgtk-3.so.0", "libgtk-3.0.dylib", "gtk-3.dll" };
        _libraryDefinitions[Library.PangoCairo] = new[] { "libpangocairo-1.0-0.dll", "libpangocairo-1.0.so.0", "libpangocairo-1.0.0.dylib", "pangocairo-1.dll" };
    }

    public static IntPtr Load(Library library)
    {
        var ret = IntPtr.Zero;
        if (_libraries.TryGetValue(library, out ret))
            return ret;

        if (FuncLoader.IsWindows)
            ret = LoadLibrary(_libraryDefinitions[library][0]);
        else if (FuncLoader.IsOSX)
            ret = LoadLibrary(_libraryDefinitions[library][2]);
        else
            ret = LoadLibrary(_libraryDefinitions[library][1]);

        if (ret == IntPtr.Zero)
        {
            for (int i = 0; i < _libraryDefinitions[library].Length; i++)
            {
                ret = LoadLibrary(_libraryDefinitions[library][i]);

                if (ret != IntPtr.Zero)
                    break;
            }
        }

        if (ret == IntPtr.Zero)
        {
            var err = library + ": " + string.Join(", ", _libraryDefinitions[library]);
            throw new DllNotFoundException(err);
        }

        _libraries[library] = ret;
        return ret;
    }

    private static IntPtr LoadLibrary(string libname)
    {
        var ret = FuncLoader.LoadLibrary(libname);
        if (ret != IntPtr.Zero)
            return ret;
        
        // Hacky solution to load libraries on Windows
        if (FuncLoader.IsWindows)
        {
            var assemblyLocation = Path.GetDirectoryName(@"C:\Users\harry\.nuget\packages\glibsharp\3.22.24.47\lib\netstandard2.0\GLibSharp.dll");
            var assemblyVersionDir = Path.GetDirectoryName(Path.GetDirectoryName(assemblyLocation));
            var version = Path.GetFileName(assemblyVersionDir);
            var gtkdir = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(assemblyVersionDir)), "gtksharp");
            var nativeLibDir = Path.Combine(gtkdir, version, "runtimes", "win-x64", "native");
            
            SetDllDirectory(nativeLibDir);

            ret = FuncLoader.LoadLibrary(Path.Combine(nativeLibDir, libname));
        }

        return ret;
    }
}