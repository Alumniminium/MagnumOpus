using System.Runtime.InteropServices;
using System.Text;

namespace MagnumOpus.Helpers
{
    public static unsafe class StringExt
    {
        public static byte* ToPointer(this string Str)
        {
            byte[] Buffer = Encoding.ASCII.GetBytes(Str + "\0");
            byte* ptr = (byte*)Malloc(Buffer.Length);

            fixed (byte* pBuffer = Buffer)
                Memcpy(ptr, pBuffer, Buffer.Length);
            return ptr;
        }

        /// <summary>
        /// Get a pointer to the Windows-1252 encoded string.
        /// It will create a null-terminating string...
        /// </summary>
        public static byte* ToPointer(this string Str, byte* ptr)
        {
            byte[] Buffer = Encoding.ASCII.GetBytes(Str + "\0");
            fixed (byte* pBuffer = Buffer)
                Memcpy(ptr, pBuffer, Buffer.Length);
            return ptr;
        }

        public static void* Malloc(int size)
        {
            void* ptr = Marshal.AllocHGlobal(size).ToPointer();
            return ptr;
        }     

        public static void Memcpy(void* dest, void* src, int size)
        {
            int count = size / sizeof(int);
            for (int i = 0; i < count; i++)
                *(((int*)dest) + i) = *(((int*)src) + i);

            int pos = size - (size % sizeof(int));
            for (int i = 0; i < size % sizeof(int); i++)
                *(((byte*)dest) + pos + i) = *(((byte*)src) + pos + i);
        }
        public static void Memcpy(byte[] dest, void* src, int size)
        {
            Marshal.Copy((IntPtr)src, dest, 0, size);
        }

        public static void Memcpy(void* dest, byte[] src, int size)
        {
            Marshal.Copy(src, 0, (IntPtr)dest, size);
        }
    }
}