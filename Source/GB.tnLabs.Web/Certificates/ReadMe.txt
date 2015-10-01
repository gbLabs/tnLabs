Azure requires a certificate for the SDK. Here is how you make a certificate:
makecert -sky exchange -r -n "CN=<CertificateName>" -pe -a sha1 -len 2048 -ss My "<CertificateName>.cer"
Also add it in the Web solution in the Certificates folder, make sure it isn't readable from the web browser.

How to create the certificates: https://www.simple-talk.com/cloud/security-and-compliance/windows-azure-management-certificates/
Also waiting on answer here: http://www.bradygaster.com/post/using-publish-settings-files-to-authenticate-the-management-libraries - actually responded but doesn't help and solved it anyway.

.cer goes onto the Azure, .pfx in the database.