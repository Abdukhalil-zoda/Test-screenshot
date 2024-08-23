using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Principal;

const string screenshotsDirectory = @"C:\Screenshots";
const string logFilePath = @"C:\Screenshots\log.txt";

// Ensure the screenshots directory exists
if (!Directory.Exists(screenshotsDirectory))
{
    Directory.CreateDirectory(screenshotsDirectory);
}

// Start capturing screenshots every minute
while (true)
{
    await CaptureAndLogScreenshotAsync();
    await Task.Delay(TimeSpan.FromMinutes(1)); // Wait for 1 minute
}


async Task CaptureAndLogScreenshotAsync()
{
    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    string fileName = $"Screenshot_{timestamp}.png";
    string filePath = Path.Combine(screenshotsDirectory, fileName);
    WindowsIdentity identity = WindowsIdentity.GetCurrent();
    try
    {
        // Capture the screen
        using (Bitmap bitmap = CaptureScreen())
        {
            // Save the screenshot
            bitmap.Save(filePath, ImageFormat.Png);
        }

        // Log the time and file information
        string logEntry = $"{DateTime.Now}:[{identity.Name}] Saved screenshot to {filePath}";
        await File.AppendAllTextAsync(logFilePath, logEntry + Environment.NewLine);
    }
    catch (Exception ex)
    {
        

        // Log any errors
        string errorLog = $"{DateTime.Now}:[{identity.Name}] Error - {ex.Message}";
        await File.AppendAllTextAsync(logFilePath, errorLog + Environment.NewLine);
    }
}
Bitmap CaptureScreen()
{
    int screenWidth = GetSystemMetrics(0); // SM_CXSCREEN
    int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN

    Bitmap bitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);

    using (Graphics g = Graphics.FromImage(bitmap))
    {
        // Use the ScreenDC approach
        IntPtr hdcSrc = GetDC(IntPtr.Zero);
        IntPtr hdcDest = g.GetHdc();

        // Bit block transfer (BitBlt) from screen DC to bitmap DC
        BitBlt(hdcDest, 0, 0, screenWidth, screenHeight, hdcSrc, 0, 0, CopyPixelOperation.SourceCopy);

        // Release the device contexts
        g.ReleaseHdc(hdcDest);
        ReleaseDC(IntPtr.Zero, hdcSrc);
    }

    return bitmap;
}

[DllImport("user32.dll", SetLastError = true)]
static extern IntPtr GetDC(IntPtr hWnd);

[DllImport("user32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

[DllImport("gdi32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, CopyPixelOperation rop);


[DllImport("user32.dll")]
static extern int GetSystemMetrics(int nIndex);