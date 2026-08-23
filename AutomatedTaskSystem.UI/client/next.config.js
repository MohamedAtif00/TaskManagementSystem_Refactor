/** @type {import('next').NextConfig} */
const nextConfig = {
    async redirects() {
    return [
      {
        source: "/projects/charts/:projectChartId",
        destination: "/subjects/charts/:projectChartId",
        permanent: false,
      },
    ];
  },
  reactStrictMode: true,
    swcMinify: true,
    generateEtags: false,
     typescript: {
    ignoreBuildErrors: true,
  },
};
module.exports = nextConfig;
  