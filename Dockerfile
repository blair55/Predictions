FROM mono:4.2.1

COPY /Predictions.Host.AppHb /static
COPY /build /build
WORKDIR /build

ENTRYPOINT ["mono", "Predictions.Api.exe"]
