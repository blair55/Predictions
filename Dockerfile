FROM mono:4.2.4

COPY /Predictions.Host.AppHb /static
COPY /build /build
WORKDIR /build

EXPOSE 9000

ENTRYPOINT ["mono", "Predictions.Api.exe"]