using AutoMapper;
using Foodico.Services.CouponAPI.Data;
using Foodico.Services.CouponAPI.Models;
using Foodico.Services.CouponAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Foodico.Services.CouponAPI
{
    public static class CouponApiEndpoints
    {
        public static void MapCouponApiEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Coupon");

            group.MapGet("/", async (AppDbContext db, IMapper mapper, ILogger<Program> logger) =>
            {
                try
                {
                    var coupons = await db.Coupons.ToListAsync();
                    var result = mapper.Map<IEnumerable<CouponDto>>(coupons);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while retrieving coupons.");
                    return Results.Problem("An unexpected error occurred. Please try again later.");
                }
            })
            .Produces<IEnumerable<CouponDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/{id:int}", async (AppDbContext db, IMapper mapper, ILogger<Program> logger, int id) =>
            {
                try
                {
                    if (id <= 0)
                    {
                        return Results.BadRequest("Invalid ID.");
                    }

                    var coupon = await db.Coupons.FindAsync(id);
                    if (coupon == null)
                    {
                        return Results.NotFound("Coupon not found.");
                    }

                    var result = mapper.Map<CouponDto>(coupon);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while retrieving the coupon.");
                    return Results.Problem("An unexpected error occurred. Please try again later.");
                }
            })
            .Produces<CouponDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapGet("/GetByCode/{code}", async (AppDbContext db, IMapper mapper, ILogger<Program> logger, string code) =>
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    return Results.BadRequest("Invalid code.");
                }

                try
                {
                    var coupon = await db.Coupons.FirstOrDefaultAsync(u => u.CouponCode.ToLower() == code.ToLower());
                    if (coupon == null)
                    {
                        return Results.NotFound("Coupon not found.");
                    }

                    var result = mapper.Map<CouponDto>(coupon);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while retrieving the coupon by code.");
                    return Results.Problem("An unexpected error occurred. Please try again later.");
                }
            })
            .Produces<CouponDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapPost("/", async (AppDbContext db, IMapper mapper, ILogger<Program> logger, CouponDto couponDto) =>
            {
                try
                {
                    if (couponDto == null || string.IsNullOrWhiteSpace(couponDto.CouponCode))
                    {
                        return Results.BadRequest("Invalid data.");
                    }
                    var coupon = mapper.Map<Coupon>(couponDto);
                    await db.Coupons.AddAsync(coupon);
                    await db.SaveChangesAsync();

                    var options = new Stripe.CouponCreateOptions
                    {
                        Name = couponDto.CouponCode,
                        Currency = "usd",
                        Id = couponDto.CouponCode,
                        AmountOff = (long)(couponDto.DiscountAmount * 100)
                    };
                    var service = new Stripe.CouponService();
                    service.Create(options);

                    var result = mapper.Map<CouponDto>(coupon);
                    return Results.Created($"/api/Coupon/{result.CouponId}", result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while creating the coupon.");
                    return Results.Problem("An unexpected error occurred. Please try again later.");
                }
            })
            .Produces<CouponDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapPut("/{id:int}", async (AppDbContext db, IMapper mapper, ILogger<Program> logger, int id, CouponDto couponDto) =>
            {
                if (id <= 0 || couponDto == null || string.IsNullOrWhiteSpace(couponDto.CouponCode))
                {
                    return Results.BadRequest("Invalid data");
                }

                try
                {
                    var coupon = await db.Coupons.FindAsync(id);
                    if (coupon == null)
                    {
                        return Results.NotFound("Coupon not found.");
                    }

                    mapper.Map(couponDto, coupon);
                    db.Coupons.Update(coupon);
                    await db.SaveChangesAsync();

                    var result = mapper.Map<CouponDto>(coupon);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while updating the coupon.");
                    return Results.Problem("An unexpected error occurred. Please try again later.");
                }
            })
            .Produces<CouponDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            group.MapDelete("/{id:int}", async (AppDbContext db, ILogger<Program> logger, int id) =>
            {
                try
                {
                    if (id <= 0)
                    {
                        return Results.BadRequest("Invalid ID");
                    }

                    var coupon = await db.Coupons.FindAsync(id);
                    if (coupon == null)
                    {
                        return Results.NotFound("Coupon not found.");
                    }

                    db.Coupons.Remove(coupon);
                    await db.SaveChangesAsync();

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while deleting the coupon.");
                    return Results.Problem("An unexpected error occurred. Please try again later.");
                }
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
