# CryptEnvVar
A simple program to encrypt small files into a blocks of base-64 encoded AES encrypted data. This can be used to store sensitive small files into a CI/CD environment variables.

## Usage
**Encrypt a file:**

`CryptEnvVar -s password123 -b 1024 < ..\..\..\Debug\netcoreapp3.1\secret_file.txt`

**Output:**
```
Block (0000)

----------------------------------------------------------------------------------------------------------------------
feMEDmA4dJMJpMnPaj+aIIuVL03YW8xtdUZStCo8gy7d2IMKRrsU6U1DDqOqj0XuSjDxCVO9SyKivQBHeaKrSfRjQZZjL2a+Mk/otNQ6Ijg7REls6GOslxom+t1grXzZU+G5Cw3osXV/lplDvmSiJ/dIDQUjFzdtUicCBMY6MRegp0tDjjsI5Bd7Mk4Sm/MtApHEMOIfeJ2Wv8O2fsx624xCzIu3sU5mxL86FPi3wD89h5Hzzjy55oDE1O4b4ZqFsxuhHqIOKWPrk3VMYGePYnh5/qvr/j0l4WFTp1ctW1kMe3dswB3pT3XElGk0ZrGx+BmSZ70AhleTwfOoefy+qAxTZUcTJmyyAlewXFUFS4SHtRDLi4Q0t1pw18k3ZT8Gms8bTJ3EkYjAWwm8NZ8FVpLQTg23JIIfrPfn7bVT841b9PzAgB5rehb3PDp2/efoN5A/trt9HdwXz4CrBfl6aS41Zyvh98A8A58yf9F0zdn56bL72bd8uTrazEzJqRInHQcsXJBBrw8TsHyj/RCcCfHeiaH5UBkzOfPeWhF9Qv6CfEErU8zOGk6JL7rEE5UOFXyt666Xs5XYSokPV2TrmckE5eAcj1hNSfJfPT7bru3NFPTJ+v17w6Eoz1pa3wdr5Pdvy9oSdOhkmd/FV5dcXe+VxPj7crTvNCkb7EvQMPPxTK3i+DI92bNz6TlB60K56iA2UVML2pS0N1XjdlfP3pSxHvAx8RBjS87kRdBJdEyGXZEo8zA4LBxQumrKI7aol9J/ackN+yTyJdJ3DG/KUIn3TRhj4Dt9BrMPRwGqDKyhhMhrwrBs0iXTwWyBhT/4f/3o5787lEgHjh1FOlQk4+GsCtEbHsrueo5bKNDuHyyMm+hnNPPjd/+vLgZP+vmmT3T1d2mw4R2YenlAwfrrzdguuOHqP2qIsFGqCAWYLyinx8anSBwmwL5U9J+AuuQVCGKiTizOU8AkBfErZkUlAwdqRhKkpORGlDT/XLuUgFnUm2YUz/Wz7f2xOgtSNwIGXlTxUWGGVNn+1HbW6RKRS3CwwVkafAZqkB38rngzpmlWJwhYbUUGlQtRj4XCGtKbxdFsz6Trni3xwlGgs4S8hZNUAmDRTinNF4BPtr5EPjLLG0NpzXnkl12XP5s0yHzR1peBa190Fx9rnRpUoExuTGKMrv0o1pIGNPEiPXUTXPVdDjnZARwQ7Wfbfa9YpfhO+Q6GEOtkwKIZHjOOLC+/ZZSNGl9+/jD2NOn/1/10AcrS3DkJXO4YShFrJ39KXLFYTm8T/q6tTOuso0P/ComLzBQqsh0PqCNSu6xP+RdZqTgPNSjx8zT7TP7eadluhtuP
----------------------------------------------------------------------------------------------------------------------

Block (0001)
----------------------------------------------------------------------------------------------------------------------

Z7+/an9KOD9k7jKS4nt0J2FRmo9lpkwUWa6TpLoW7E1X7PUv2uZY74z5skxNkHGvoCUUv41YxU8F7iPhqSA+nl5pjvwRtfLKzYsv4+YBaR0eF3fmTmTWJ+miBMFlqakFr/u7e8M7HKmnF8gxAQlPrV2wMJQgvcY4LS9rOnbjnwsWPf8Hwy0YIaHigia9yrEDNGzeAasbXTyxSb6P4oRSrS36rsM0+9eK9j6Hl365iGJdbLPlp6qZAPsUblhDtE6saCKZU59zVZrYlcVlG1Q3kf0gPRc9t4Zpejcb7ykRdJ+4y6nTYZzOSWrLsPALrnlOJ5pJ3wYKlBoc4A52PZBPgCwvBcFlMlqOXfMKhWqEXveyi7Frnz+YKVjuHDkhUAjU5m4gjIsY30QYBXGYGV1DCK0ArVoI2XruzoFl33uc1ApLycx6JKnKsxVQfhfKpJ1D99fltoT9ee4R1fmqIc/4/AjxauPhRiRD1jCz9ZPX4eLCeQGCZbY9lxOfLVaPCUQ14hYaXdVTAzZbOKK/M2Oel2QeytH3AVUNN4TWKmVEjhRzyVacgCAfdZtpniZzAhUfoFqPTXMxvretBN8fSEr0KPVSJ9EUoXKFcyGEeesll8Btj5fS/ZN+uQX8S9MAAE3UCsEjAebQp1jKsUN/eYi9XmSysYeJuq/EfH+2U538vO8p9mgtRJE1pk/FQoEDObQV7Tnz+4QYx/VY+0i96UIcXi0198sKjIUWTaeqsolfkhiXBAeCRFY+UZi2HXQIIAAotKnplYtgQOKwdTF332RVQw==*
----------------------------------------------------------------------------------------------------------------------
```
**Decrypt a file from an environment variable:**

`CryptEnvVar -s password123 -e var1;var2 > decrypt.txt`

**Help:**

`CryptEnvVar --help`

**Output:**
```
CryptEnvVar 1.0.3
Copyright © VPKSoft 2020

ERROR(S):
  Required option 's, secret' is missing.

  -s, --secret              Required. The secret to encrypt or decrypt data.

  -f, --file                The file name to use.

  -b, --block               The amount of encrypted data to include in a single
                            base-64 encoded block. The default is 1024.

  -w, --width               The width of the console.

  -e, --environment         Environment variables to decrypt. This is a
                            semicolon (';') delimited list.

  -i, --ignoreRedirect      A flag indicating whether to ignore console
                            input/output redirection in case of a CI/CD
                            environment.

  -v, --verbose             A flag indicating whether display as much output as
                            possible.

  --help                    Display this help screen.

  --version                 Display version information.
```
