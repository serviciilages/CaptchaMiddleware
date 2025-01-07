To use this middleware you have to declare in the Program.cs

builder.Services.AddCaptcha();

and to use it

app.UseCaptcha();

Next you need to place the attribute you want for the controller method you want to veryfy the capcha
