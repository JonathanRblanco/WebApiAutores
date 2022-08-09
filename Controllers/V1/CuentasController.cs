using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.Dtos;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        //private readonly HashService hashService;
        //private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager
            /*IDataProtectionProvider dataProtectionProvider*/, HashService hashService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            //this.hashService = hashService;
            //dataProtector =dataProtectionProvider.CreateProtector("valor_unico");
        }
        //[HttpGet("encriptar")]
        //public ActionResult Encriptar()
        //{
        //    var textoPLano = "Jonathan Blanco";
        //    var textoEncriptado = dataProtector.Protect(textoPLano);
        //    var textoDesencriptado = dataProtector.Unprotect(textoEncriptado);
        //    return Ok(new
        //    {
        //        TextoPlano = textoPLano,
        //        TextoEncriptado = textoEncriptado,
        //        TextoDesencriptado = textoDesencriptado
        //    });
        //}
        //[HttpGet("encriptarPorTiempo")]
        //public ActionResult EncriptarPorTiempo()
        //{
        //    var protectorLimitadoPorTiempo = dataProtector.ToTimeLimitedDataProtector();
        //    var textoPLano = "Jonathan Blanco";
        //    var textoEncriptado = protectorLimitadoPorTiempo.Protect(textoPLano,lifetime:TimeSpan.FromMinutes(5));
        //    var textoDesencriptado = protectorLimitadoPorTiempo.Unprotect(textoEncriptado);
        //    return Ok(new
        //    {
        //        TextoPlano = textoPLano,
        //        TextoEncriptado = textoEncriptado,
        //        TextoDesencriptado = textoDesencriptado
        //    });
        //}
        //[HttpGet("hash/{textoPlano}")]
        //public ActionResult RealizarHashing(string textoPlano)
        //{
        //    var resultado1 = hashService.Hash(textoPlano);
        //    var resultado2 = hashService.Hash(textoPlano);
        //    return Ok(new
        //    {
        //        textoPlano=textoPlano,
        //        Resultado1 = resultado1,
        //        Resultado2 = resultado2
        //    });
        //}

        [HttpPost("registrar", Name = "registrarUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CreadencialesUsuario creadencialesUsuario)
        {
            var user = new IdentityUser
            {
                UserName = creadencialesUsuario.Email,
                Email = creadencialesUsuario.Email
            };
            var resultado = await userManager.CreateAsync(user, creadencialesUsuario.Password);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(creadencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }
        [HttpPost("login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CreadencialesUsuario creadencialesUsuario)
        {
            var resultado = await signInManager.PasswordSignInAsync(creadencialesUsuario.Email, creadencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(creadencialesUsuario);
            }
            else
            {
                return BadRequest("Login incorrecto");
            }
        }
        [HttpGet("RenovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(c => c.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var credencialesUsuario = new CreadencialesUsuario()
            {
                Email = email
            };
            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CreadencialesUsuario creadencialesUsuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("email",creadencialesUsuario.Email)
            };
            var usuario = await userManager.FindByEmailAsync(creadencialesUsuario.Email);
            var claimsBd = await userManager.GetClaimsAsync(usuario);
            claims.AddRange(claimsBd);
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var expiracion = DateTime.UtcNow.AddMinutes(30);
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);
            return new RespuestaAutenticacion
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }
        [HttpPost("HacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }
        [HttpPost("RemoverAdmin", Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }
    }
}
