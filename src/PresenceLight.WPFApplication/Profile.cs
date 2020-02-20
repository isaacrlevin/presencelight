using System;

namespace PresenceLight.WPFApplication
{
    public class Rootobject
    {
        public string odatacontext { get; set; }
        public string id { get; set; }
        public object deletedDateTime { get; set; }
        public bool accountEnabled { get; set; }
        public object ageGroup { get; set; }
        public string[] businessPhones { get; set; }
        public string city { get; set; }
        public object createdDateTime { get; set; }
        public object creationType { get; set; }
        public string companyName { get; set; }
        public object consentProvidedForMinor { get; set; }
        public object country { get; set; }
        public string department { get; set; }
        public string displayName { get; set; }
        public object employeeId { get; set; }
        public object faxNumber { get; set; }
        public string givenName { get; set; }
        public string[] imAddresses { get; set; }
        public object isResourceAccount { get; set; }
        public string jobTitle { get; set; }
        public object legalAgeGroupClassification { get; set; }
        public string mail { get; set; }
        public string mailNickname { get; set; }
        public object mobilePhone { get; set; }
        public string onPremisesDistinguishedName { get; set; }
        public string officeLocation { get; set; }
        public string onPremisesDomainName { get; set; }
        public string onPremisesImmutableId { get; set; }
        public DateTime onPremisesLastSyncDateTime { get; set; }
        public string onPremisesSecurityIdentifier { get; set; }
        public string onPremisesSamAccountName { get; set; }
        public bool onPremisesSyncEnabled { get; set; }
        public string onPremisesUserPrincipalName { get; set; }
        public object[] otherMails { get; set; }
        public string passwordPolicies { get; set; }
        public object passwordProfile { get; set; }
        public object postalCode { get; set; }
        public string preferredDataLocation { get; set; }
        public object preferredLanguage { get; set; }
        public string[] proxyAddresses { get; set; }
        public DateTime refreshTokensValidFromDateTime { get; set; }
        public object showInAddressList { get; set; }
        public DateTime signInSessionsValidFromDateTime { get; set; }
        public object state { get; set; }
        public object streetAddress { get; set; }
        public string surname { get; set; }
        public string usageLocation { get; set; }
        public string userPrincipalName { get; set; }
        public object externalUserState { get; set; }
        public object externalUserStateChangeDateTime { get; set; }
        public string userType { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_SupervisorInd { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_ZipCode { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_StateProvinceCode { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToPersonnelNbr { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToFullName { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_ReportsToEmailName { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_ProfitCenterCode { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_PositionNumber { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_CountryShortCode { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_CostCenterCode { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_CityName { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_BuildingName { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_BuildingID { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_AddressLine1 { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_LocationAreaCode { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_CompanyCode { get; set; }
        public string extension_18e31482d3fb4a8ea958aa96b662f508_PersonnelNumber { get; set; }
        public Assignedlicens[] assignedLicenses { get; set; }
        public Assignedplan[] assignedPlans { get; set; }
        public Devicekey[] deviceKeys { get; set; }
        public Identity[] identities { get; set; }
        public Onpremisesextensionattributes onPremisesExtensionAttributes { get; set; }
        public object[] onPremisesProvisioningErrors { get; set; }
        public Provisionedplan[] provisionedPlans { get; set; }
    }

    public class Onpremisesextensionattributes
    {
        public object extensionAttribute1 { get; set; }
        public string extensionAttribute2 { get; set; }
        public object extensionAttribute3 { get; set; }
        public string extensionAttribute4 { get; set; }
        public string extensionAttribute5 { get; set; }
        public object extensionAttribute6 { get; set; }
        public object extensionAttribute7 { get; set; }
        public object extensionAttribute8 { get; set; }
        public object extensionAttribute9 { get; set; }
        public object extensionAttribute10 { get; set; }
        public object extensionAttribute11 { get; set; }
        public string extensionAttribute12 { get; set; }
        public object extensionAttribute13 { get; set; }
        public object extensionAttribute14 { get; set; }
        public object extensionAttribute15 { get; set; }
    }

    public class Assignedlicens
    {
        public string[] disabledPlans { get; set; }
        public string skuId { get; set; }
    }

    public class Assignedplan
    {
        public DateTime assignedDateTime { get; set; }
        public string capabilityStatus { get; set; }
        public string service { get; set; }
        public string servicePlanId { get; set; }
    }

    public class Devicekey
    {
        public string keyType { get; set; }
        public string keyMaterial { get; set; }
        public string deviceId { get; set; }
    }

    public class Identity
    {
        public string signInType { get; set; }
        public string issuer { get; set; }
        public string issuerAssignedId { get; set; }
    }

    public class Provisionedplan
    {
        public string capabilityStatus { get; set; }
        public string provisioningStatus { get; set; }
        public string service { get; set; }
    }
}

