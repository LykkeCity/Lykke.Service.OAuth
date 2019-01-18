using System.Threading.Tasks;

namespace Core.ExternalProvider
{
    /// <summary>
    ///     Temporary storage for data.
    /// </summary>
    public interface IUserSession
    {
        /// <summary>
        ///     Save value to session.
        /// </summary>
        /// <remarks>
        ///     Creates session if it not exist.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Completed Task on success.</returns>
        Task SetAsync<T>(string key, T value);

        /// <summary>
        ///     Get value from current session.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Saved value or null if value or session does not exist.</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        ///     Delete value from session.
        /// </summary>
        /// <remarks>
        ///     Recreates session with same id upon successful deletion.
        /// </remarks>
        /// <param name="key">Key</param>
        /// <returns>Completed Task on success.</returns>
        Task DeleteAsync(string key);

        /// <summary>
        /// Clears session and data.
        /// </summary>
        /// <returns>Completed Task on success.</returns>
        Task EndSessionAsync();
    }
}
