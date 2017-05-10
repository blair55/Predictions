FROM mono:4.8.0.495

COPY /Predictions.Host.AppHb /static
COPY /build /build
WORKDIR /build

EXPOSE 9000

ENTRYPOINT ["mono", "Predictions.Api.exe"]