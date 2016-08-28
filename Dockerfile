FROM mono:4.2.1

RUN echo "deb http://download.mono-project.com/repo/debian wheezy/snapshots/4.2.1.102 main" > /etc/apt/sources.list.d/mono-xamarin.list \
	&& apt-get update \
	&& apt-get install -y build-essential mono-complete \
	&& rm -rf /var/lib/apt/lists/*
COPY /Predictions.Host.AppHb /static
COPY /build /build
WORKDIR /build

ENTRYPOINT ["mono", "Predictions.Api.exe"]
