using Entities;

namespace RepositoryContracts
{

    /// <summary>
    /// Represents data access logic for managing Country entity.
    /// </summary>
    public interface ICountriesRepository
    {
        /// <summary>
        /// Adds a new country object to the data store.
        /// </summary>
        /// <param name="country"> Country object to add</param>
        /// <returns>Returns the country object after adding it to the data store.</returns>
        Task<Country> AddCountry(Country country);

        /// <summary>
        /// Returns all countries in the data store.
        /// </summary>
        /// <returns>Return all the countries from the table.</returns>
        Task<List<Country>> GetAllCountries();

        /// <summary>
        /// Returns a country object based on the given country id; otherwise it returns null.
        /// </summary>
        /// <param name="CountryID">CountryID to search</param>
        /// <returns>Matching country or null</returns>
        Task<Country?> GetCountryByCountryID(Guid CountryID);

        /// <summary>
        /// Returns a country object based on given country name.
        /// </summary>
        /// <param name="CountryName">Country name to search. </param>
        /// <returns></returns>
        Task<Country?> GetCountryByCountryName(string CountryName);
    }
}
