using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ContextMenuEditor.Utilities;

/// <summary>
/// Helper class for resolving Windows resource strings from DLLs.
/// Uses P/Invoke to call SHLoadIndirectString which properly resolves
/// strings like @shell32.dll,-8506 to their actual display text.
/// </summary>
public static class ResourceStringResolver
{
    // P/Invoke declaration for SHLoadIndirectString
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SHLoadIndirectString(
        string pszSource,
        StringBuilder pszOutBuf,
        int cchOutBuf,
        IntPtr ppvReserved);

    /// <summary>
    /// Resolves a Windows resource string to its display text.
    /// Handles formats like @shell32.dll,-8506 or C:\path\to.dll.-123
    /// </summary>
    /// <param name="resourceString">The resource string to resolve</param>
    /// <returns>The resolved display text, or the original string if resolution fails</returns>
    public static string ResolveResourceString(string resourceString)
    {
        if (string.IsNullOrWhiteSpace(resourceString))
            return resourceString;

        // Check if this looks like a resource string
        if (!resourceString.StartsWith("@") && !resourceString.Contains(".-"))
            return resourceString;

        // Normalize the format - SHLoadIndirectString expects @dllpath,-resourceID
        string normalizedString = resourceString;
        
        // Convert format "C:\path\to.dll.-123" to "@C:\path\to.dll,-123"
        if (normalizedString.Contains(".-"))
        {
            normalizedString = "@" + normalizedString.Replace(".-", ",-");
        }

        try
        {
            const int maxStringLength = 1024;
            var outBuffer = new StringBuilder(maxStringLength);
            
            int result = SHLoadIndirectString(normalizedString, outBuffer, maxStringLength, IntPtr.Zero);
            
            if (result == 0 && outBuffer.Length > 0)
            {
                return outBuffer.ToString();
            }
        }
        catch (Exception ex)
        {
            // If P/Invoke fails, log and fall back to original string
            System.Diagnostics.Debug.WriteLine($"Failed to resolve resource string '{resourceString}': {ex.Message}");
        }

        // If resolution failed, return original string cleaned up
        return CleanupUnresolvedString(resourceString);
    }

    private static string CleanupUnresolvedString(string value)
    {
        // Remove @ prefix if present
        value = value.TrimStart('@');
        
        // If it still looks like a path with resource ID, extract something meaningful
        if (value.Contains(".-") || value.Contains(",-"))
        {
            var parts = value.Split(new[] { ".-", ",-" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                // Extract just the filename without extension
                var fileName = System.IO.Path.GetFileNameWithoutExtension(parts[0]);
                return $"{fileName} (Resource String)";
            }
        }

        return value;
    }
}
