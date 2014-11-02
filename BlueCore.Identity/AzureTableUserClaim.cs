using System;
using System.Security.Claims;

namespace BlueCore.Identity.AzureTableStorage
{
    /// <summary>
    /// A Claim is a statement about an entity by an Issuer.
    /// A Claim consists of a Value, a Subject and an Issuer.
    /// Additional properties, Type, ValueType, Properties and OriginalIssuer 
    /// help understand the claim when making decisions.
    /// </summary>
    [Serializable]
    public class AzureTableUserClaim : Claim
    {
        public AzureTableUserClaim() : base(string.Empty, string.Empty) { }
        
        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type and value.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.Issuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>,        
        /// <see cref="Claim.ValueType"/> is set to <see cref="ClaimValueTypes.String"/>, 
        /// <see cref="Claim.OriginalIssuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>, and
        /// <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public AzureTableUserClaim(string type, string value)
            : base(type, value)
        {
        }

        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, and value type.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.Issuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>,
        /// <see cref="Claim.OriginalIssuer"/> is set to <see cref="ClaimsIdentity.DefaultIssuer"/>,
        /// and <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>        
        /// <seealso cref="ClaimValueTypes"/>
        public AzureTableUserClaim(string type, string value, string valueType)
            : base(type, value, valueType)
        {
        }

        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, value type, and issuer.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type. If this parameter is empty or null, then <see cref="ClaimValueTypes.String"/> is used.</param>
        /// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="ClaimsIdentity.DefaultIssuer"/> is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.OriginalIssuer"/> is set to value of the <paramref name="issuer"/> parameter,
        /// <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public AzureTableUserClaim(string type, string value, string valueType, string issuer)
            : base(type, value, valueType, issuer, issuer)
        {
        }

        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, value type, issuer and original issuer.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type. If this parameter is null, then <see cref="ClaimValueTypes.String"/> is used.</param>
        /// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="ClaimsIdentity.DefaultIssuer"/> is used.</param>
        /// <param name="originalIssuer">The original issuer of this claim. If this parameter is empty or null, then orignalIssuer == issuer.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <remarks>
        /// <see cref="Claim.Subject"/> is set to null.
        /// </remarks>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public AzureTableUserClaim(string type, string value, string valueType, string issuer, string originalIssuer)
            : base(type, value, valueType, issuer, originalIssuer)
        {
        }
        
        /// <summary>
        /// Creates a <see cref="Claim"/> with the specified type, value, value type, issuer and original issuer.
        /// </summary>
        /// <param name="type">The claim type.</param>
        /// <param name="value">The claim value.</param>
        /// <param name="valueType">The claim value type. If this parameter is null, then <see cref="ClaimValueTypes.String"/> is used.</param>
        /// <param name="issuer">The claim issuer. If this parameter is empty or null, then <see cref="ClaimsIdentity.DefaultIssuer"/> is used.</param>
        /// <param name="originalIssuer">The original issuer of this claim. If this parameter is empty or null, then orignalIssuer == issuer.</param>
        /// <param name="subject">The subject that this claim describes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is null.</exception>
        /// <seealso cref="ClaimsIdentity"/>
        /// <seealso cref="ClaimTypes"/>
        /// <seealso cref="ClaimValueTypes"/>
        public AzureTableUserClaim(string type, string value, string valueType, string issuer, string originalIssuer, ClaimsIdentity subject)
            : base( type, value, valueType, issuer, originalIssuer, subject )
        {
        }
    }
}