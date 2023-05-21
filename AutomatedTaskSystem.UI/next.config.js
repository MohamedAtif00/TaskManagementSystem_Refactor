/** @type {import('next').NextConfig} */
const nextConfig = {
	reactStrictMode: true,
	swcMinify: true,
	generateEtags: false,
	redirects: async () => [
		{
			source: "/",
			destination: "/tasks",
			permanent: false,
		},
	],
};
module.exports = nextConfig;
