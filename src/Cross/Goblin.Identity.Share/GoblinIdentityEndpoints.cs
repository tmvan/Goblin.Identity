namespace Goblin.Identity.Share
{
    public static class GoblinIdentityEndpoints
    {
        // ------------------------------------------------------------------------
        // Users
        // ------------------------------------------------------------------------
        
        /// <summary>
        ///     HTTP Method: POST
        /// </summary>
        public const string RegisterUser = "/users";
        
        /// <summary>
        ///     HTTP Method: GET
        /// </summary>
        public const string GetProfile = "/users/{id}";
        
        /// <summary>
        ///     HTTP Method: PUT
        /// </summary>
        public const string UpdateProfile = "/users/{id}";
        
        /// <summary>
        ///     HTTP Method: DELETE
        /// </summary>
        public const string DeleteUser = "/users/{id}";
        
        /// <summary>
        ///     HTTP Method: POST
        /// </summary>
        public const string GetPagedUser = "/users/paged";
        
        /// <summary>
        ///     HTTP Method: POST
        /// </summary>
        public const string RequestConfirmEmail = "/users/{id}/confirm-email";

        /// <summary>
        ///     HTTP Method: PUT
        /// </summary>
        public const string ConfirmEmail = "/users/{id}/confirm-email";

        /// <summary>
        ///     HTTP Method: PUT
        /// </summary>
        public const string UpdateIdentity = "/users/{id}/identity";

        /// <summary>
        ///     HTTP Method: POST
        /// </summary>
        public const string GenerateAccessToken = "/access-tokens";
        
        /// <summary>
        ///     HTTP Method: GET
        /// </summary>
        public const string GetProfileByAccessToken = "/access-tokens";
        
        /// <summary>
        ///     HTTP Method: POST
        /// </summary>
        public const string RequestResetPassword = "/request-reset-password";
        
        /// <summary>
        ///     HTTP Method: PUT
        /// </summary>
        public const string ResetPassword = "/reset-password";
        
        // ------------------------------------------------------------------------
        // Roles
        // ------------------------------------------------------------------------
        
        /// <summary>
        ///     HTTP Method: GET
        /// </summary>
        public const string GetAllRoles = "/roles";
        
        /// <summary>
        ///     HTTP Method: GET
        /// </summary>
        public const string GetRole = "/roles/{name}";
        
        /// <summary>
        ///     HTTP Method: POST
        /// </summary>
        public const string UpsertRole = "/roles";
        
        /// <summary>
        ///     HTTP Method: GET
        /// </summary>
        public const string GetAllPermissions = "/permissions";
    }
}