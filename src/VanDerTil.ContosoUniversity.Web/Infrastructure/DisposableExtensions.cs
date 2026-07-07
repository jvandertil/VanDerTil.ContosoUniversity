namespace VanDerTil.ContosoUniversity.Web.Infrastructure;

public static class DisposableExtensions
{
    extension(IAsyncDisposable disposable)
    {
        /// <summary>
        /// Asynchronously disposes the underlying resource, suppressing any exceptions that occur during disposal.
        /// </summary>
        /// <remarks>This method ensures that disposal is attempted and any exceptions thrown during
        /// disposal are ignored. Use this method when it is important to guarantee that disposal does not propagate
        /// exceptions to the caller.</remarks>
        /// <returns>A ValueTask that represents the asynchronous dispose operation.</returns>
        public async ValueTask SafeDisposeAsync()
        {
            try
            {
                await disposable.DisposeAsync();
            }
            catch
            {
                // The purpose of this method is to safely dispose without throwing exceptions.
            }
        }
    }
}
