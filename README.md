# FreeboxWhitelist

Calculate blacklist from whitelisted numbers

The Freebox Revolution offers a blacklist feature, where you can reject incoming calls if they start with a chosen prefix.

However there is no way to set up a whitelist and reject all other numbers.

As the number of nuisance calls is growing all the time, this app calculates a blacklist that will reject most incoming calls apart from the whitelisted numbers.

## Where can I get it?

It is built on Appveyor here:

https://ci.appveyor.com/project/dastevens/freeboxwhitelist/build/artifacts

Download the zip file and extract all the contents.

## How do I run it?

It's an exe with a config file. Edit the config file with your whitelist

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
  </startup>
  <appSettings>
    <!-- Default value 5 -->
    <!--add key="MaxFilterLength" value="9"/-->

    <!-- Add numbers or prefixes to whitelist here -->
    <add key="Number:0123456789" />
    <add key="Number:0987654321" />
  </appSettings>
</configuration>
```

Then run it. It calculates the blacklist and prints the codes you need to use to program the freebox.

Then it simulates random incoming numbers and calculates how many it would reject.
